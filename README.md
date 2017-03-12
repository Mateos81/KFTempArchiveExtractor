# KFTempArchiveExtractor
Manually extracts the Killing Floor Steam Workshop items the game couldn't extract itself...

## Original behaviour
This project is based upon Steam user **cmicroc**'s guide [The Internal File Format of the Workshop TempArchiveXX Files](http://steamcommunity.com/sharedfiles/filedetails/?id=291724762).

Simply drag and drop a **TempArchiveX** file upon the executable to get its content extracted, with a prompt as to either extract/overwrite each file.
I've just added a sub-folder creation and a pause so the user can read the console output.

## New behaviour
Simply execute the program; it may ask for Admin rights, because it will read the KF installation location from the Windows registry.
It will then create a sub-folder **KF Archive Files**, and in it a sub-folder per file.
