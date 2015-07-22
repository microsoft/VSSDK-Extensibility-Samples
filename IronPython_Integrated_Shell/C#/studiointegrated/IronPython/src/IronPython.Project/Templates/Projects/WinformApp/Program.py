from System import *
from System.Windows.Forms import *
from Form1 import *

class $safeprojectname$Program: # namespace
    
    @staticmethod
    def RealEntryPoint():
        Application.EnableVisualStyles()
        Application.Run($safeprojectname$.Form1())

if __name__ == "Program":
    $safeprojectname$Program.RealEntryPoint();