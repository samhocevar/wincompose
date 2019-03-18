#!/bin/sh

_exit=0

if git grep 'Icon=.*[.]ico' >/dev/null; then
    echo "ERROR! do not use .ico files as icons (crashes on .NET 3.5)"
    git grep 'Icon=.*[.]ico'
    _exit=1
fi

exit $_exit

