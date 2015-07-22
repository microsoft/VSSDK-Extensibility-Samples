import System
from System.Web import *
from System.Web.Services import *
from System.Web.Services.Protocols import *
from System.ComponentModel import *
from clr import *

class $safeprojectname$: #namespace
    #At present, IronPython does not support attributes. The following are the C#
    #equivalents required for a Web Service
    #[WebService(Namespace = "http://tempuri.org/")]
    #[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    #[ToolboxItem(false)]
    class Service1(System.Web.Services.WebService):
        
        #[WebMethod]
        def HelloWorld(self):
            return "Hello World"
        
    
