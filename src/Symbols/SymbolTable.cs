using SuperBasic.FrontEnd.Inter;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperBasic.FrontEnd.Symbols
{
    /*
     * SymbolTable 符号表
     * 
     * 此符号表维护一个带HashCode的列表，
     */
    public class SymbolTable
    {
        public static List<SymbolTable> list = new List<SymbolTable>();
        private static int index = -1;

        private Dictionary<string, Id> table;
        private SymbolTable prev;
        private List<SymbolTable> sub;
        public Stmt Belong { get; protected set; }
        public int Index { get; protected set; }
        public string Name { get; protected set; }

        /// <summary>
        /// 初始化符号表实例
        /// </summary>
        /// <param name="p">父符号表</param>
        public SymbolTable(SymbolTable p, string name = "")
        {
            Name = name;
            Index = ++index;
            sub = new List<SymbolTable>();
            table = new Dictionary<string, Id>();
            prev = p;
            prev?.Add(this);
            list.Add(this);
        }

        public SymbolTable(SymbolTable p, Stmt belong, string name = "") : this(p, name)
        {
            Belong = belong;
        }

        public void Add(string s, Id sym)
        {
            table.Add(s, sym);
        }

        public IDictionary<string, Id> Table
        {
            get { return table; }
        }

        public override string ToString()
        {
            return Name.Length == 0 ? "_scope_" + Index : Name;
        }

        public IEnumerable<SymbolTable> SubTables
        {
            get { return sub; }
        }

        public void Add(SymbolTable t)
        {
            sub.Add(t);
        }

        public SymbolTable Parent
        {
            get { return prev; }
        }

        public bool Exist(string s)
        {
            return Table.ContainsKey(s);
        }

        public IEnumerable<Id> Members
        {
            get { return table.Values; }
        }

        public Id this[string s]
        {
            get
            {
                for(SymbolTable t = this; t != null; t = t.Parent)
                {
                    Id result = t.Table.ContainsKey(s) ? t.Table[s] : null;
                    if (result != null) return result;
                }
                return null;
            }
        }
    }
}
