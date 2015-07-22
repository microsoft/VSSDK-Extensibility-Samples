import System
from System.Windows.Forms import *
from System.ComponentModel import *
from System.Drawing import *
from clr import *

class $safeprojectname$: #namespace
    class Form1(System.Windows.Forms.Form):
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
            #  Form1
            # 
            #  
            # 
            self.ClientSize = Size(292, 266)
            self.Name = 'Form1'
            self.Text = 'Form1'
            self.ResumeLayout(False)
            self.PerformLayout()
