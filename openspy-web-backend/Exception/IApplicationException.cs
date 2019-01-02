using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Exception
{
    public partial class IApplicationException : System.Exception
    {
        public String _class;
        public String _name;
        public Dictionary<String, String> _extra;
        public IApplicationException(String _class, String _name) : base(_name)
        {
            this._class = _class;
            this._name = _name;
        }
    }
}
