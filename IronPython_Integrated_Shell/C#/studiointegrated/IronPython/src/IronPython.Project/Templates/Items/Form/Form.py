import System
from System.Windows.Forms import *
from System.ComponentModel import *
from System.Drawing import *
from clr import *

class $rootnamespace$: # namespace
    class $safeitemname$(System.Windows.Forms.Form):
        """"""
        __slots__ = []
        
        def __init__(self):
            self.InitializeComponent()
        
        @accepts(object, bool)
        @returns(None)
        def Dispose(self, disposing):
            #if disposing and (components != None):
            #    components.Dispose()
            
            super(type(self), self).Dispose(disposing)
        
        @returns(None)
        def InitializeComponent(self):
            self.SuspendLayout()
            #  
            # 
            #  $safeitemname$
            # 
            #  
            # 
            self.ClientSize = Size(292, 266)
            self.Name = '$safeitemname$'
            self.Text = '$safeitemname$'
            self.ResumeLayout(False)
            self.PerformLayout()
