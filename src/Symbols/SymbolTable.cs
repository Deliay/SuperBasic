using SuperBasic.FrontEnd.Inter;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperBasic.FrontEnd.Symbols
{
    /*
    class Symbol
    {
        string identity;
        Type type;
        object data;

        public Symbol(string identity, Type type)
        {
            this.identity = identity;
            this.type = type;
        }

        public Symbol(string id, Type t, object d) : this(id, t)
        {
            data = d;
        }

        public string Identity
        {
            get { return identity; }
        }

        public Type Type
        {
            get { return type; }
        }

        public object Data
        {
            get { return data; }
        }
    }
    */
    //old symbol implement

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
        private string name;
        public int Index { get; protected set; }

        /// <summary>
        /// 初始化符号表实例
        /// </summary>
        /// <param name="p">父符号表</param>
        public SymbolTable(SymbolTable p, string name = "")
        {
            this.name = name;
            Index = ++index;
            sub = new List<SymbolTable>();
            table = new Dictionary<string, Id>();
            prev = p;
            prev?.Add(this);
            list.Add(this);
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
            return name.Length == 0 ? "_scope_" + Index : name;
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
