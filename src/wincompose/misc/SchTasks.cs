//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2021 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace WinCompose
{
    internal static class SchTasks
    {
        public static bool HasTask(string task_name)
            => !string.IsNullOrEmpty(RunSchTasks("/query /tn {task_name} /xml"));

        public static void InstallTask(string task_name)
        {
            var cmd = $"\"\"\"\"{Utils.ExecutableName}\"\"\" -fromtask\"";
            var source = $@"{Environment.SystemDirectory}\Tasks\{task_name}";
            // Save temporary task file to our app data dir in case we want to inspect it.
            Utils.EnsureDirectory(Utils.AppDataDir);
            var tmp = Path.Combine(Utils.AppDataDir, "task.backup.xml");

            try
            {
                // Create a scheduled task, then edit the resulting XML with some
                // features that the command line tool does not support, and reload
                // the XML file.
                RunSchTasks($"/tn {task_name} /f /create /sc onlogon /tr {cmd}");

                var doc = new XmlDocument();
                doc.Load(source);
                FixTaskElement(doc.DocumentElement);
                doc.Save(tmp);

                RunSchTasks($"/tn {task_name} /f /create /xml \"{tmp}\"");
            }
            catch (Exception ex)
            {
                Log.Warn($"Could not create scheduled task: {ex}");
            }
        }

        private static Dictionary<string, string> m_xml_renames = new Dictionary<string, string>()
        {
            // Make sure we use a GroupId, not a UserId; we can’t use the SYSTEM
            // account because it is not allowed to open GUI programs. We use the
            // built-in BUILTIN\Users group instead.
            { "UserId", "GroupId" },
        };

        private static HashSet<string> m_xml_removes = new HashSet<string>()
        {
            "LogonType", // This tag is only legal for UserId.
        };

        private static Dictionary<string, string> m_xml_replaces = new Dictionary<string, string>()
        {
            { "Author", "Sam Hocevar" },
            { "RunLevel", "HighestAvailable" },        // run with higest privileges
            { "ExecutionTimeLimit", "PT0S" },          // allow to run indefinitely
            { "MultipleInstancesPolicy", "Parallel" }, // allow multiple instances
            { "DisallowStartIfOnBatteries", "false" },
            { "StopIfGoingOnBatteries", "false" },
            { "StopOnIdleEnd", "false" },
            { "StartWhenAvailable", "false" },
            { "RunOnlyIfNetworkAvailable", "false" },
            { "GroupId", GetLocalUserGroupName() },
        };

        private static void FixTaskElement(XmlElement node)
        {
            // Rename node if necessary
            if (m_xml_renames.TryGetValue(node.Name, out string new_name))
            {
                var tmp = node.OwnerDocument.CreateElement(new_name, node.NamespaceURI);
                foreach (XmlNode child in node.ChildNodes)
                    tmp.AppendChild(child.CloneNode(true));
                node.ParentNode.InsertBefore(tmp, node);
                node.ParentNode.RemoveChild(node);
                node = tmp;
            }

            // Replace node content if necessary
            if (m_xml_replaces.TryGetValue(node.Name, out string content))
                node.InnerText = content;

            // Recurse
            if (node.HasChildNodes && node.FirstChild is XmlElement first_child)
                FixTaskElement(first_child);

            // Process next sibling
            if (node.NextSibling is XmlElement sibling)
                FixTaskElement(sibling);

            // Remove node if necessary (make sure to do this after sibling)
            if (m_xml_removes.Contains(node.Name))
                node.ParentNode.RemoveChild(node);
        }

        private static string GetLocalUserGroupName()
        {
            var user_name = new StringBuilder();
            var domain_name = new StringBuilder();
            user_name.EnsureCapacity(128);
            domain_name.EnsureCapacity(128);
            var user_size = (uint)user_name.Capacity;
            var domain_size = (uint)domain_name.Capacity;
            // Build SID S-1-5-32-545 (“Users” group)
            byte[] sid = new byte[]
            {
                1, // Revision
                2, // SubAuthorityCount
                0, 0, 0, 0, 0, 5, // IdentifierAuthority = SECURITY_NT_AUTHORITY (5)
                32, 0, 0, 0, // SECURITY_BUILTIN_DOMAIN_RID (32)
                33, 2, 0, 0, // DOMAIN_ALIAS_RID_USERS (545)
            };

            if (!NativeMethods.LookupAccountSid(null, sid, user_name, ref user_size, domain_name,
                                                ref domain_size, out SID_NAME_USE sid_use))
                return @"BUILTIN\Users";

            return $@"{domain_name}\{user_name}";
        }


        private static string RunSchTasks(string args)
        {
            var pi = new ProcessStartInfo()
            {
                FileName = "schtasks.exe",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            var p = Process.Start(pi);
            p.WaitForExit();
            return p.ExitCode == 0 ? p.StandardOutput.ReadToEnd() : null;
        }
    }
}
