using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caro_vovanlinh
{
     public class PLayer
     {
         private string name;
         public string Name
         {
             get { return name; }
             set { name = value; }
         }
         private Image mark;
         public Image Mark
         {
             get { return mark; }
             set { mark = value; }
         }
         public PLayer(string name, Image mark)
         {
             this.Name = name;
             this.Mark = mark;
         }
     }
   
}
