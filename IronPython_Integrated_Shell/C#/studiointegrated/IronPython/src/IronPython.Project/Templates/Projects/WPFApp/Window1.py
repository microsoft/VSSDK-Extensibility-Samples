from System import *
from System.IO import *
from System.Windows import *
from clr import *
import System
import System.Windows

class $safeprojectname$: # namespace
    class $safeitemname$:
        """type(Root) == System.Windows.Window"""
        __slots__ = [ 'Root' ]
        
        def __init__(self):
            self.Root = Application.LoadComponent(Uri("/$safeprojectname$;component/$safeitemname$.xaml", UriKind.Relative))
