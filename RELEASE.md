Release Howto for WinCompose
============================

① Ensure the version information is up to date
----------------------------------------------

Files to edit:

 - `src/build.config`
 - `README.md` (preemptively)

Make sure to run `./update-data.sh` so that translations are up to date.

② Build installer and portable versions
---------------------------------------

Just run `make` in an MSYS2 shell. Building the Visual Studio solution
will not be enough, as it only builds the installer.

③ Commit, tag and push
----------------------

④ Upload packages
-----------------

Create a [new release](https://github.com/samhocevar/wincompose/releases)
on GitHub and upload the `.exe` and `.zip` files.

⑤ Update website for the automatic updater
------------------------------------------

The queried URL is http://wincompose.info/status.txt

⑥ Advertise release on news websites
------------------------------------

 * [Portable Freeware](http://www.portablefreeware.com/?id=2615) (updates need to be requested [in the forums](http://www.portablefreeware.com/forums/viewforum.php?f=8))
 * [Softpedia](http://www.softpedia.com/get/System/OS-Enhancements/WinCompose.shtml) (there is a “send us an update” button)

