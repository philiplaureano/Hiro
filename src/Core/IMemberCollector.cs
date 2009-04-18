using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiro
{
    public interface IMemberCollector
    {
        void AddMember<TMember>(TMember member);
    }
}
