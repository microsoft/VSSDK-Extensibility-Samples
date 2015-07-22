from System import *
from System.IO import *
from System.Windows import *
from Window1 import *

class $safeprojectname$App: # namespace
    @staticmethod
    def RealEntryPoint():
        a = Application()
        window1 = $safeprojectname$.Window1()
        a.Run(window1.Root)

if __name__ == "Program":
    $safeprojectname$App.RealEntryPoint();