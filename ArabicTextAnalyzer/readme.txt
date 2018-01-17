Download cygwin : https://www.cygwin.com/setup-x86_64.exe
1. Configuring the .sh script on the machine:
1.1 Cygwin x64 (not x86) has to be installed on the machine with the devel package : switch from 'default' to 'install' (8Gb !!!)
1.2 C:\cygwin64\bin has to be added as a PATH variable in user path but top
1.3 The .sh script must be ran within the containing folder (as it uses relative paths to \example and others)
1.4 .sh file has to be configured to be opened using C:\cygwin64\bin\bash.exe (so that just by clicking it, it should function properly)
1.5 new sys variable : CYGWIN_HOME : C:\cygwin64

2. Configure app to use IIS
1.1 Add website under IIS (with proper access for that website folder to IUSR and IIS_USRS users)
1.2 Configure the AppPool that uses the website to have 'Custom' identity (by clicking advanced settings)
    That identity must be set to the user of the machine (oadrjnlparbzprl/....) in order to have access to run it.

