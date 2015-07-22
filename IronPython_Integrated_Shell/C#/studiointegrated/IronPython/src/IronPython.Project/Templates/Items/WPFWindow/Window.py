from System import *
from System.IO import *
from System.Windows import *
from clr import *
import System
import System.Windows

class $rootnamespace$: # namespace
    class $safeitemrootname$:
        """type(Root) == System.Windows.Window"""
        __slots__ = [ 'Root' ]
        
        def __init__(self):
            self.Root = Application.LoadComponent(Uri("/$rootnamespace$;component/$safeitemrootname$.xaml", UriKind.Relative))
