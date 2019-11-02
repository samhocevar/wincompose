//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2019 Sam Hocevar <sam@hocevar.net>
//              2014—2015 Benjamin Litzelmann
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.ComponentModel;

namespace WinCompose
{
    /// <summary>
    /// The base class to represent an entry in the settings file. It handles
    /// thread-safe and process-safe saving and loading.
    /// </summary>
    public abstract class SettingsEntry
    {
        protected SettingsEntry(object defaultValue)
        {
            m_value = defaultValue;
        }

        public event Action ValueChanged;

        /// <summary>
        /// Serializes the current value into a <see cref="string"/>. This
        /// method should not throw any unhandled exception.
        /// </summary>
        /// <returns>A string representing the given value.</returns>
        public override string ToString()
        {
            // The default implementation just uses the ToString method
            return m_value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Deserializes the given string into an object of the type of this
        /// entry. This method should not throw any unhandled exception.
        /// </summary>
        /// <param name="str">The string to deserialize.</param>
        public abstract void LoadString(string str);

        protected void OnValueChanged() => ValueChanged?.Invoke();

        protected object m_value;
    }

    /// <summary>
    /// A generic implementation of the <see cref="SettingsEntry"/> class. It
    /// handles serialization for most of the .NET built-in types.
    /// </summary>
    /// <typeparam name="T">The type of data this entry contains.</typeparam>
    public class SettingsEntry<T> : SettingsEntry
    {
        public SettingsEntry(T defaultValue)
            : base(defaultValue)
        {
        }

        /// <summary>
        /// Gets or sets the value of this settings entry.
        /// </summary>
        public T Value
        {
            get => (T)m_value;
            set
            {
                if (m_value == null || !m_value.Equals(value))
                {
                    m_value = value;
                    OnValueChanged();
                }
            }
        }

        /// <inheritdoc/>
        public override void LoadString(string str)
        {
            try
            {
                var cv = TypeDescriptor.GetConverter(typeof(T));
                // Fall back to the Convert class if there is no converter.
                m_value = cv.CanConvertFrom(typeof(string))
                        ? (T)cv.ConvertFrom(str)
                        : (T)Convert.ChangeType(str, typeof(T));
            }
            catch (Exception)
            {
            }
        }
    }
}
