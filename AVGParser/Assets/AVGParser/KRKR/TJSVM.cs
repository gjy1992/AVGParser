using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AVGParser.KRKR
{
    public enum VarType
    {
        VOID,
        NUMBER,
        STRING,
        ARRAY,
        DICTIONARY,
        FUNCTION,
        CLASS,
        CLOSURE,
        UNDEFINED
    }

    enum Operator
    {
        OP_IF,
        OP_ELSE,
        OP_COMMA,
        OP_SET,
        OP_SETBITAND,   //&=
        OP_SETBITOR,    //|=
        OP_SETBITXOR,   //^=
        OP_SETSUB,
        OP_SETADD,
        OP_SETMOD,
        OP_SETDIV,
        OP_SETINTDIV,
        OP_SETMUL,
        OP_SETAND,      //&&=
        OP_SETOR,       //||=
        OP_SETLEFTSHIFT,
        OP_SETRIGHTSHIFT,
        OP_QUESTION,
        OP_COLON,

        OP_BITAND,      //&
        OP_BITOR,       //|
        OP_BITNOT,      //~
        OP_SUB,
        OP_ADD,
        OP_MOD,
        OP_DIV,
        OP_INTDIV,
        OP_MUL,
        OP_AND,         //&&
        OP_OR,          //||
        OP_LEFTSHIFT,
        OP_RIGHTSHIFT,
        OP_EQUAL,
        OP_STRICTEQUAL,
        OP_NOTEQUAL,
        OP_NOTSTRICTEQUAL,
        OP_LARGER,
        OP_LARGEREQUAL,
        OP_SMALLER,
        OP_SMALLEREQUAL,
        OP_BITXOR,      //^

        OP_NOT,
        OP_DEC,
        OP_INC,
        OP_NEW,
        OP_INSTANCEOF,
        OP_TYPEOF,
        OP_DELETE,
        OP_CHAR,        //#
        OP_DOLLAR,      //$
        OP_PARENTHESES, //(
        OP_PARENTHESES2,//)
        OP_BRACKET,     //[
        OP_BRACKET2,    //]
        OP_BRACE,       //{
        OP_BRACE2,      //}
        OP_DIC,         //%[
        OP_DOT,
        OP_INT,
        OP_REAL,
        OP_STRING,
        OP_SWITCH,
        OP_CASE,
        OP_DEFAULT,
        OP_CLASS,
        OP_FUNCTION,
        OP_RETURN,
        OP_BREAK,
        OP_CONTINUE,
        OP_SEMICOLON,    //;
        OP_VALUE,       //=>
        OP_FOR,
        OP_WHILE,
        OP_DO,
        OP_VAR,

        OP_CONST,
        OP_LITERAL,

        OP_TRUE,
        OP_FALSE,
        OP_GLOBAL,
        OP_THIS,
        OP_SUPER,

        OP_END
    }

    enum VMCode
    {
        //the sequence is same with Operator
        VM_BITAND,
        VM_BITOR,
        VM_BITNOT,
        VM_SUB,
        VM_ADD,
        VM_MOD,
        VM_DIV,
        VM_INTDIV,
        VM_MUL,
        VM_AND,
        VM_OR,
        VM_LEFTSHIFT,
        VM_RIGHTSHIFT,

        VM_EQUAL,
        VM_STRICTEQUAL,
        VM_NOTEQUAL,
        VM_NOTSTRICTEQUAL,
        VM_LARGER,
        VM_LARGEREQUAL,
        VM_SMALLER,
        VM_SMALLEREQUAL,

        VM_BITXOR,
        VM_NOT,

        VM_DOT,         //a=b.c
        VM_DOTSET,      //a.b=c
        VM_DOTSETVOID,      //a.b=void

        VM_LOADCONST,   //a=const[b]
        VM_COPY,        //a=b
        VM_LOADVOID,    //a=void
        VM_LOADTRUE,
        VM_LOADFALSE,


        VM_CALL,        //a(b,b+1,...,c-1)
        VM_JUMP,
        VM_RETURN,      //return a
        VM_RETURNVOID,      //return

        VM_JUMPFALSE,   //if(!a)jump
        VM_JUMPTRUE,   //if(!a)jump

        VM_TOINT,
        VM_TOMINUSNUM,
        VM_TOSTRING,
        VM_TONUMBER,

        VM_INC,         //a=++b
        VM_DEC,
        VM_POSTINC,     //a=b++
        VM_POSTDEC,

        VM_CHAR,
        VM_STR,         //$
        VM_MAKEARRAY,   //[a,a+1,...,b-1]
        VM_MAKEDIC,     //[a,a+1,...,b-1]
        VM_MAKEFUNC,    //[a,b,...,c-1],a is function, b... is default param
        VM_SETFUNCCLOSURE,  //set closure for [a]

        VM_NEW,
        VM_TYPEOF,
        VM_DELETE,      //detele a.b
        VM_INSTANCEOF,

        VM_GLOBAL,
        VM_SUPERDOT,

        VM_REGVAR,      //stack[a] <=> var consts[b]
        VM_REGUPVALUE,  //consts[a] is upvalue

        VM_NULL,
    }

    class TJSException : Exception
    {
        internal struct Trace
        {
            internal int line;
            internal int offset;
        }

        internal List<Trace> traces = new List<Trace>();

        public TJSException(string msg) : base(msg)
        {
        }

        public void AddTrace(int line, int lineoffset)
        {
            traces.Add(new Trace { line = line, offset = lineoffset });
        }

        public override string StackTrace
        {
            get
            {
                string trace = "";
                for (int i = 0; i < traces.Count; i++)
                {
                    trace += string.Format("\nin Line {0}, Pos{1}", traces[i].line, traces[i].offset);
                }
                return trace;
            }
        }
    }

    public abstract class TJSObject
    {
        public abstract VarType GetVarType();

        public virtual TJSVariable GetField(string name)
        {
            throw new TJSException($"member {name} not exist");
        }

        public virtual TJSVariable GetField(int name)
        {
            throw new TJSException($"member {name} not exist");
        }

        public virtual void SetField(string name, TJSVariable value)
        {
            throw new TJSException($"cannot set value of member {name}");
        }

        public virtual void SetField(int name, TJSVariable value)
        {
            throw new TJSException($"cannot set value of member {name}");
        }

        public virtual void RemoveField(string name)
        {
            throw new TJSException($"cannot remove member {name}");
        }

        public virtual void RemoveField(int name)
        {
            throw new TJSException($"cannot remove member {name}");
        }

        public virtual TJSVariable CallAsFunc(TJSVariable[] param, int start, int length)
        {
            throw new TJSException("cannot be called");
        }

        public virtual string ConvertToString()
        {
            return ToString() + GetHashCode().ToString();
        }
    }

    public class TJSFunction : TJSObject
    {
        internal TJSClosure closure = null;

        internal TJSVariable _this;

        internal Func<TJSVariable, TJSVariable[], int, int, TJSVariable> nativefunc = null;

        internal TJSIL ilcode = null;

        internal TJSVariable[] defaultParam = null;

        public TJSFunction(TJSFunction func)
        {
            closure = func.closure;
            _this = func._this;
            nativefunc = func.nativefunc;
            ilcode = func.ilcode;
            defaultParam = func.defaultParam;
        }

        public TJSFunction(Func<TJSVariable, TJSVariable[], int, int, TJSVariable> native, TJSVariable _this)
        {
            nativefunc = native;
            this._this = _this;
        }

        public TJSFunction(TJSFunction tJS, TJSClosure closure, TJSVariable _this)
        {
            nativefunc = tJS.nativefunc;
            ilcode = tJS.ilcode;
            defaultParam = tJS.defaultParam;
            this.closure = closure;
            this._this = _this;
        }

        internal TJSFunction(TJSIL code, TJSClosure closure)
        {
            ilcode = code;
            this.closure = closure;
        }

        public override TJSVariable CallAsFunc(TJSVariable[] param, int start, int length)
        {
            if (nativefunc != null)
                return nativefunc.Invoke(_this, param, start, length);
            if (defaultParam == null)
            {
                return TJSVM.VM.Run(ilcode, closure, _this, null);
            }
            else
            {
                var p = new TJSVariable[defaultParam.Length];
                for (int i = 0; i < p.Length; i++)
                {
                    if (i < length)
                    {
                        p[i] = param[i + start];
                    }
                    else
                    {
                        p[i] = defaultParam[i];
                    }
                }
                return TJSVM.VM.Run(ilcode, closure, _this, p);
            }
        }

        public override VarType GetVarType()
        {
            return VarType.FUNCTION;
        }
    }

    public abstract class TJSClosure : TJSObject
    {
        internal TJSClosure parent;

        public TJSClosure(TJSClosure p)
        {
            parent = p;
        }

        public abstract TJSVariable getMember(string name);

        public abstract bool hasMember(string name);

        public abstract void setMember(string name, TJSVariable value);

        public override TJSVariable GetField(string name)
        {
            if (hasMember(name))
                return getMember(name);
            if (parent != null)
                return parent.GetField(name);
            return new TJSVariable(VarType.UNDEFINED);
        }

        public override TJSVariable GetField(int name)
        {
            return GetField(name.ToString());
        }

        public override VarType GetVarType()
        {
            return VarType.CLOSURE;
        }

        public override void SetField(string name, TJSVariable value)
        {
            if (hasMember(name))
            {
                setMember(name, value);
                return;
            }
            if (parent != null)
            {
                parent.SetField(name, value);
                return;
            }
            setMember(name, value);
        }

        public override void SetField(int name, TJSVariable value)
        {
            SetField(name.ToString(), value);
        }
    }

    public class TJSNormalClosure : TJSClosure
    {
        internal Dictionary<string, TJSVariable> dic = new Dictionary<string, TJSVariable>();

        public TJSNormalClosure(TJSClosure p) : base(p)
        {

        }

        public override TJSVariable getMember(string name)
        {
            return dic[name];
        }

        public override bool hasMember(string name)
        {
            return dic.ContainsKey(name);
        }

        public override void setMember(string name, TJSVariable value)
        {
            dic[name] = value;
        }

        public override void RemoveField(int name)
        {
            RemoveField(name.ToString());
        }

        public override void RemoveField(string name)
        {
            if (dic.ContainsKey(name))
                dic.Remove(name);
        }
    }

    public class TJSStackClosure : TJSClosure
    {
        internal Dictionary<string, int> localvar = null;

        internal class UpValueClass
        {
            internal TJSVariable[] stack;
            internal int index;
        }

        internal Dictionary<string, UpValueClass> upvalues = null;

        internal TJSVariable[] localStack = null;

        public TJSStackClosure(TJSClosure p) : base(p)
        {

        }

        public override TJSVariable getMember(string name)
        {
            return localStack[localvar[name]];
        }

        public override bool hasMember(string name)
        {
            return localvar != null && localvar.ContainsKey(name);
        }

        public override void setMember(string name, TJSVariable value)
        {
            localStack[localvar[name]] = value;
        }

        bool QueryUpValue(string name, out TJSVariable[] stack, out int index)
        {
            if (localvar != null)
            {
                if (localvar.ContainsKey(name))
                {
                    stack = localStack;
                    index = localvar[name];
                    return true;
                }
                if (parent != null && (parent is TJSStackClosure))
                {
                    return ((TJSStackClosure)parent).QueryUpValue(name, out stack, out index);
                }
            }
            stack = null;
            index = 0;
            return false;
        }

        public void RegUpValue(string name)
        {
            if (parent == null || !(parent is TJSStackClosure))
                return;
            bool res = ((TJSStackClosure)parent).QueryUpValue(name, out TJSVariable[] stack, out int index);
            if (res)
            {
                if (upvalues == null)
                    upvalues = new Dictionary<string, UpValueClass>();
                upvalues[name] = new UpValueClass { index = index, stack = stack };
            }
        }
    }

    public class TJSArray : TJSObject
    {
        internal List<TJSVariable> arr;

        public TJSArray(List<TJSVariable> v)
        {
            arr = v;
        }

        public override VarType GetVarType()
        {
            return VarType.ARRAY;
        }

        public override TJSVariable GetField(int name)
        {
            if (name >= arr.Count || name < -arr.Count)
                return new TJSVariable(VarType.UNDEFINED);
            if (name < 0)
                name += arr.Count;
            return arr[name];
        }

        public override void SetField(int name, TJSVariable value)
        {
            if (name >= arr.Count)
            {
                while (arr.Count < name)
                {
                    arr.Add(new TJSVariable(VarType.VOID));
                }
                arr.Add(value);
            }
            else if (name >= 0)
            {
                arr[name] = value;
            }
            else if (name >= -arr.Count)
            {
                arr[name + arr.Count] = value;
            }
            //else return
        }

        public override TJSVariable GetField(string name)
        {
            int idx = 0;
            bool r = int.TryParse(name, out idx);
            if (r)
                return GetField(idx);
            else
                return new TJSVariable(VarType.UNDEFINED);
        }

        public override void SetField(string name, TJSVariable value)
        {
            int idx = 0;
            bool r = int.TryParse(name, out idx);
            if (r)
                SetField(idx, value);
            throw new TJSException("invalid index");
        }

        public override void RemoveField(int name)
        {
            if (name >= arr.Count || name < -arr.Count)
                throw new TJSException("index overflow");
            if (name < 0)
                name += arr.Count;
            arr.RemoveAt(name);
        }

        public override void RemoveField(string name)
        {
            int idx = 0;
            bool r = int.TryParse(name, out idx);
            if (r)
                RemoveField(idx);
            throw new TJSException("invalid index");
        }
    }

    public class TJSDictionary : TJSObject
    {
        internal Dictionary<string, TJSVariable> dic;

        public TJSDictionary(Dictionary<string, TJSVariable> v)
        {
            dic = v;
        }

        public override TJSVariable GetField(string name)
        {
            if (dic.ContainsKey(name))
                return dic[name];
            return new TJSVariable(VarType.UNDEFINED);
        }

        public override TJSVariable GetField(int name)
        {
            return GetField(name.ToString());
        }

        public override VarType GetVarType()
        {
            return VarType.DICTIONARY;
        }

        public override void SetField(string name, TJSVariable value)
        {
            dic[name] = value;
        }

        public override void SetField(int name, TJSVariable value)
        {
            SetField(name.ToString(), value);
        }

        public override void RemoveField(int name)
        {
            RemoveField(name.ToString());
        }

        public override void RemoveField(string name)
        {
            if (dic.ContainsKey(name))
                dic.Remove(name);
        }
    }

    public class TJSClass : TJSNormalClosure
    {
        internal List<TJSClass> parentClass;

        internal bool isInstance = false;

        internal string name;

        internal Dictionary<string, TJSVariable> defindeVars = new Dictionary<string, TJSVariable>();

        internal Dictionary<string, TJSVariable> propAndFunc = new Dictionary<string, TJSVariable>();

        public TJSClass() : base(null)
        {

        }

        public bool IsInctanceOf(string name)
        {
            if (this.name == name || name == "object")
                return true;
            foreach (var it in parentClass)
            {
                if (it.IsInctanceOf(name))
                    return true;
            }
            return false;
        }

        public override VarType GetVarType()
        {
            return VarType.CLASS;
        }

        public bool hasMemberFunc(string name, out TJSVariable v)
        {
            if (propAndFunc.ContainsKey(name))
            {
                v = propAndFunc[name];
                return true;
            }
            if (parentClass != null)
            {
                foreach (var p in parentClass)
                {
                    if (p.hasMemberFunc(name, out v))
                        return true;
                }
            }
            v = new TJSVariable();
            return false;
        }

        public override TJSVariable GetField(int name)
        {
            return GetField(name.ToString());
        }

        public override TJSVariable GetField(string name)
        {
            if (propAndFunc.ContainsKey(name))
                return propAndFunc[name];
            if (defindeVars.ContainsKey(name))
                return defindeVars[name];
            if (parentClass != null)
            {
                TJSVariable v;
                foreach (var p in parentClass)
                {
                    if (p.hasMemberFunc(name, out v))
                    {
                        if (v.vt == VarType.FUNCTION)
                        {
                            v = new TJSVariable(new TJSFunction((TJSFunction)v.obj, this, new TJSVariable(this)));
                            return v;
                        }
                    }
                }
            }
            throw new TJSException($"member {name} not exist");
        }

        public override void SetField(int name, TJSVariable value)
        {
            SetField(name.ToString(), value);
        }

        public override void SetField(string name, TJSVariable value)
        {
            defindeVars[name] = value;
        }

        public override void RemoveField(int name)
        {
            RemoveField(name.ToString());
        }

        public override void RemoveField(string name)
        {
            if (propAndFunc.ContainsKey(name))
                propAndFunc.Remove(name);
        }
    }

    class TJSStringContext : TJSObject
    {
        public string str;

        public override VarType GetVarType()
        {
            return VarType.CLASS;
        }

        public override TJSVariable GetField(int name)
        {
            string newstr = "";
            if (name >= 0 && name < str.Length)
                newstr += str[name];
            else if (name >= -str.Length && name < 0)
                newstr += str[name + str.Length];
            else
                throw new TJSException("index overflow");
            return new TJSVariable(newstr);
        }

        public override TJSVariable GetField(string name)
        {
            switch (name)
            {
                case "length":
                    return new TJSVariable(str.Length);
                default:
                    {
                        TJSVariable v;
                        if (TJSVM.VM.strClass.hasMemberFunc(name, out v))
                        {
                            if (v.vt == VarType.FUNCTION)
                            {
                                return new TJSVariable(new TJSFunction((TJSFunction)v.obj, null, new TJSVariable(str)));
                            }
                        }
                    }
                    {
                        bool r = false;
                        int v;
                        r = int.TryParse(name, out v);
                        if (r)
                            return GetField(v);
                    }
                    break;
            }
            throw new TJSException($"member {name} not exist");
        }

        public override string ConvertToString()
        {
            return str;
        }
    }

    class TJSNumberContext : TJSObject
    {
        public double num;

        public override VarType GetVarType()
        {
            return VarType.CLASS;
        }

        public override string ConvertToString()
        {
            return num.ToString();
        }

        public override TJSVariable GetField(string name)
        {
            switch (name)
            {
                default:
                    {
                        TJSVariable v;
                        if (TJSVM.VM.numClass.hasMemberFunc(name, out v))
                        {
                            if (v.vt == VarType.FUNCTION)
                            {
                                return new TJSVariable(new TJSFunction((TJSFunction)v.obj, null, new TJSVariable(num)));
                            }
                        }
                    }
                    break;
            }
            throw new TJSException($"member {name} not exist");
        }
    }

    class TJSArrayContext : TJSObject
    {
        public TJSArray arr;

        public override VarType GetVarType()
        {
            return VarType.CLASS;
        }

        public override TJSVariable GetField(int name)
        {
            if (name >= 0 && name < arr.arr.Count)
                return arr.arr[name];
            else if (name >= -arr.arr.Count && name < 0)
                return arr.arr[name + arr.arr.Count];
            else
                return new TJSVariable();
        }

        public override TJSVariable GetField(string name)
        {
            switch (name)
            {
                case "length":
                    return new TJSVariable(arr.arr.Count);
                default:
                    {
                        TJSVariable v;
                        if (TJSVM.VM.arrClass.hasMemberFunc(name, out v))
                        {
                            if (v.vt == VarType.FUNCTION)
                            {
                                return new TJSVariable(new TJSFunction((TJSFunction)v.obj, null, new TJSVariable(arr)));
                            }
                        }
                    }
                    {
                        bool r = false;
                        int v;
                        r = int.TryParse(name, out v);
                        if (r)
                            return GetField(v);
                    }
                    break;
            }
            throw new TJSException($"member {name} not exist");
        }

        public override void SetField(int name, TJSVariable v)
        {
            if (name >= 0)
            {
                if (name >= arr.arr.Count)
                {
                    if (name >= 100000)
                        throw new TJSException("array size exceed 100000");
                    arr.arr.AddRange(Enumerable.Repeat(new TJSVariable(VarType.VOID), name + 1 - arr.arr.Count));
                }
                arr.arr[name] = v;
            }
            else
            {
                if (name < -arr.arr.Count)
                {
                    if (name < -100000)
                        throw new TJSException("array size exceed 100000");
                    arr.arr.AddRange(Enumerable.Repeat(new TJSVariable(VarType.VOID), -name - arr.arr.Count));
                }
                arr.arr[name + arr.arr.Count] = v;
            }
        }

        public override void SetField(string name, TJSVariable value)
        {
            switch (name)
            {
                case "length":
                    {
                        int l = value.ToInt();
                        if (l < 0)
                            throw new TJSException("length cannot < 0");
                        if (l < arr.arr.Count)
                        {
                            arr.arr.RemoveRange(l, arr.arr.Count - l);
                        }
                        else if (l > arr.arr.Count)
                        {
                            arr.arr.AddRange(Enumerable.Repeat(new TJSVariable(VarType.VOID), l - arr.arr.Count));
                        }
                    }
                    break;
                default:
                    {
                        bool r = false;
                        int v;
                        r = int.TryParse(name, out v);
                        if (r)
                            SetField(v, value);
                    }
                    break;
            }
        }
    }

    class TJSDictionaryContext : TJSObject
    {
        public TJSDictionary dic;

        public override VarType GetVarType()
        {
            return VarType.CLASS;
        }

        public override TJSVariable GetField(int name)
        {
            return GetField(name.ToString());
        }

        public override TJSVariable GetField(string name)
        {
            switch (name)
            {
                case "length":
                    return new TJSVariable(dic.dic.Count);
                default:
                    {
                        TJSVariable v;
                        if (TJSVM.VM.dicClass.hasMemberFunc(name, out v))
                        {
                            if (v.vt == VarType.FUNCTION)
                            {
                                return new TJSVariable(new TJSFunction((TJSFunction)v.obj, null, new TJSVariable(dic)));
                            }
                        }
                    }
                    return dic.GetField(name);
            }
        }

        public override void SetField(int name, TJSVariable value)
        {
            SetField(name.ToString(), value);
        }

        public override void SetField(string name, TJSVariable value)
        {
            switch (name)
            {
                default:
                    dic.SetField(name, value);
                    break;
            }
        }
    }

    class TJSNode
    {
        internal Operator op;
        internal List<TJSNode> children;
        internal TJSVariable variable;
        internal string name;

        internal int line;
        internal int pos;

        public void AddChild(TJSNode node)
        {
            if (children == null)
                children = new List<TJSNode>();
            children.Add(node);
        }

        public void AddChild(TJSNode node1, TJSNode node2)
        {
            if (children == null)
                children = new List<TJSNode>();
            children.Add(node1);
            children.Add(node2);
        }
    }

    class TJSLexer
    {
        Dictionary<string, Operator> operators = new Dictionary<string, Operator>();

        string[] toParse;

        int line;
        int pos;

        bool nextIsBareWord;

        public TJSLexer()
        {
            operators["if"] = Operator.OP_IF;
            operators["else"] = Operator.OP_ELSE;
            operators["new"] = Operator.OP_NEW;
            operators["instanceof"] = Operator.OP_INSTANCEOF;
            operators["typeof"] = Operator.OP_TYPEOF;
            operators["delete"] = Operator.OP_DELETE;
            operators["int"] = Operator.OP_INT;
            operators["real"] = Operator.OP_REAL;
            operators["string"] = Operator.OP_STRING;
            operators["switch"] = Operator.OP_SWITCH;
            operators["case"] = Operator.OP_CASE;
            operators["default"] = Operator.OP_DEFAULT;
            operators["function"] = Operator.OP_FUNCTION;
            operators["return"] = Operator.OP_RETURN;
            operators["class"] = Operator.OP_CLASS;
            operators["break"] = Operator.OP_BREAK;
            operators["continue"] = Operator.OP_CONTINUE;
            operators["this"] = Operator.OP_THIS;
            operators["super"] = Operator.OP_SUPER;
            operators["true"] = Operator.OP_TRUE;
            operators["false"] = Operator.OP_FALSE;
            operators["global"] = Operator.OP_GLOBAL;
            operators["for"] = Operator.OP_FOR;
            operators["while"] = Operator.OP_WHILE;
            operators["do"] = Operator.OP_DO;
            operators["var"] = Operator.OP_VAR;
        }

        public void LoadScript(string s)
        {
            s = s.Replace("\r\n", "\n");
            s = s.Replace("\r", "\n");
            toParse = s.Split('\n');

            line = 0;
            pos = 0;
            nextIsBareWord = false;
        }

        double ParseNumber()
        {
            int start = pos;
            bool haveE = false;
            bool haveDot = false;
            while (pos < toParse[line].Length)
            {
                if (char.IsDigit(toParse[line][pos]))
                {
                    pos++;
                }
                else if (toParse[line][pos] == '.')
                {
                    if (haveDot)
                        break;
                    haveDot = true;
                    pos++;
                }
                else if (toParse[line][pos] == 'E' || toParse[line][pos] == 'e')
                {
                    if (haveE)
                        break;
                    if (pos + 1 < toParse[line].Length && (toParse[line][pos + 1] == '+' || toParse[line][pos + 1] == '-'))
                        pos += 2;
                    else
                        pos++;
                }
                else
                {
                    break;
                }
            }
            double res;
            double.TryParse(toParse[line].Substring(start, pos - start), out res);
            return res;
        }

        public TJSNode ReadToken()
        {
            TJSNode node = new TJSNode();
            while (pos < toParse[line].Length && char.IsWhiteSpace(toParse[line][pos]))
                pos++;
            while (line < toParse.Length && pos >= toParse[line].Length)
            {
                line++;
                pos = 0;
                if (line >= toParse.Length)
                    break;
                while (pos < toParse[line].Length && char.IsWhiteSpace(toParse[line][pos]))
                    pos++;
            }
            node.line = line;
            node.pos = pos;
            if (line >= toParse.Length)
            {
                node.op = Operator.OP_END;
                return node;
            }
            switch (toParse[line][pos])
            {
                case ',':
                    node.op = Operator.OP_COMMA;
                    pos++;
                    return node;
                case '=':
                    if (pos + 1 < toParse[line].Length)
                    {
                        if (toParse[line][pos + 1] == '=')
                        {
                            if (pos + 2 < toParse[line].Length && toParse[line][pos + 2] == '=')
                            {
                                node.op = Operator.OP_STRICTEQUAL;
                                pos += 3;
                                return node;
                            }
                            node.op = Operator.OP_EQUAL;
                            pos += 2;
                            return node;
                        }
                        else if (toParse[line][pos + 1] == '>')
                        {
                            node.op = Operator.OP_VALUE;
                            pos += 2;
                            return node;
                        }
                    }
                    node.op = Operator.OP_SET;
                    pos++;
                    return node;
                case '&':
                    if (pos + 1 < toParse[line].Length)
                    {
                        if (toParse[line][pos + 1] == '&')
                        {
                            if (pos + 2 < toParse[line].Length && toParse[line][pos + 2] == '=')
                            {
                                node.op = Operator.OP_SETAND;
                                pos += 3;
                                return node;
                            }
                            node.op = Operator.OP_AND;
                            pos += 2;
                            return node;
                        }
                        else if (toParse[line][pos + 1] == '=')
                        {
                            node.op = Operator.OP_SETBITAND;
                            pos += 2;
                            return node;
                        }
                    }
                    node.op = Operator.OP_BITAND;
                    pos++;
                    return node;
                case '|':
                    if (pos + 1 < toParse[line].Length)
                    {
                        if (toParse[line][pos + 1] == '|')
                        {
                            if (pos + 2 < toParse[line].Length && toParse[line][pos + 2] == '=')
                            {
                                node.op = Operator.OP_SETOR;
                                pos += 3;
                                return node;
                            }
                            node.op = Operator.OP_OR;
                            pos += 2;
                            return node;
                        }
                        else if (toParse[line][pos + 1] == '=')
                        {
                            node.op = Operator.OP_SETBITOR;
                            pos += 2;
                            return node;
                        }
                    }
                    node.op = Operator.OP_BITOR;
                    pos++;
                    return node;
                case '^':
                    if (pos + 1 < toParse[line].Length && toParse[line][pos + 1] == '=')
                    {
                        node.op = Operator.OP_SETBITXOR;
                        pos += 2;
                        return node;
                    }
                    node.op = Operator.OP_BITXOR;
                    pos++;
                    return node;
                case '+':
                    if (pos + 1 < toParse[line].Length)
                    {
                        if (toParse[line][pos + 1] == '=')
                        {
                            node.op = Operator.OP_SETADD;
                            pos += 2;
                            return node;
                        }
                        else if (toParse[line][pos + 1] == '+')
                        {
                            node.op = Operator.OP_INC;
                            pos += 2;
                            return node;
                        }
                    }
                    node.op = Operator.OP_ADD;
                    pos++;
                    return node;
                case '-':
                    if (pos + 1 < toParse[line].Length)
                    {
                        if (toParse[line][pos + 1] == '=')
                        {
                            node.op = Operator.OP_SETSUB;
                            pos += 2;
                            return node;
                        }
                        else if (toParse[line][pos + 1] == '+')
                        {
                            node.op = Operator.OP_DEC;
                            pos += 2;
                            return node;
                        }
                    }
                    node.op = Operator.OP_SUB;
                    pos++;
                    return node;
                case '%':
                    if (pos + 1 < toParse[line].Length)
                    {
                        if (toParse[line][pos + 1] == '=')
                        {
                            node.op = Operator.OP_SETMOD;
                            pos += 2;
                            return node;
                        }
                        else if (toParse[line][pos + 1] == '[')
                        {
                            node.op = Operator.OP_DIC;
                            pos += 2;
                            return node;
                        }
                    }
                    node.op = Operator.OP_MOD;
                    pos++;
                    return node;
                case '/':
                    if (pos + 1 < toParse[line].Length)
                    {
                        if (toParse[line][pos + 1] == '*')
                        {
                            pos += 2;
                            while (line < toParse.Length)
                            {
                                while (pos + 1 < toParse[line].Length)
                                {
                                    if (toParse[line][pos] == '*' && toParse[line][pos + 1] == '/')
                                    {
                                        pos += 2;
                                        break;
                                    }
                                }
                                pos = 0;
                                line++;
                            }
                            if (pos >= toParse[line].Length)
                            {
                                pos = 0;
                                line++;
                            }
                            return ReadToken();
                        }
                        else if (toParse[line][pos + 1] == '/')
                        {
                            line++;
                            pos = 0;
                            return ReadToken();
                        }
                        else if (toParse[line][pos] == '=')
                        {
                            node.op = Operator.OP_SETDIV;
                            pos += 2;
                            return node;
                        }
                    }
                    node.op = Operator.OP_DIV;
                    pos++;
                    return node;
                case '\\':
                    if (pos + 1 < toParse[line].Length && toParse[line][pos + 1] == '=')
                    {
                        node.op = Operator.OP_SETINTDIV;
                        pos += 2;
                        return node;
                    }
                    node.op = Operator.OP_INTDIV;
                    pos++;
                    return node;
                case '*':
                    if (pos + 1 < toParse[line].Length && toParse[line][pos + 1] == '=')
                    {
                        node.op = Operator.OP_SETMUL;
                        pos += 2;
                        return node;
                    }
                    node.op = Operator.OP_MUL;
                    pos++;
                    return node;
                case '>':
                    if (pos + 1 < toParse[line].Length)
                    {
                        if (toParse[line][pos + 1] == '>')
                        {
                            if (pos + 2 < toParse[line].Length && toParse[line][pos + 2] == '=')
                            {
                                node.op = Operator.OP_SETRIGHTSHIFT;
                                pos += 3;
                                return node;
                            }
                            node.op = Operator.OP_RIGHTSHIFT;
                            pos += 2;
                            return node;
                        }
                        else if (toParse[line][pos + 1] == '=')
                        {
                            node.op = Operator.OP_LARGEREQUAL;
                            pos += 2;
                            return node;
                        }
                    }
                    node.op = Operator.OP_LARGER;
                    pos++;
                    return node;
                case '<':
                    if (pos + 1 < toParse[line].Length)
                    {
                        if (toParse[line][pos + 1] == '<')
                        {
                            if (pos + 2 < toParse[line].Length && toParse[line][pos + 2] == '=')
                            {
                                node.op = Operator.OP_SETLEFTSHIFT;
                                pos += 3;
                                return node;
                            }
                            node.op = Operator.OP_LEFTSHIFT;
                            pos += 2;
                            return node;
                        }
                        else if (toParse[line][pos + 1] == '=')
                        {
                            node.op = Operator.OP_SMALLEREQUAL;
                            pos += 2;
                            return node;
                        }
                    }
                    node.op = Operator.OP_SMALLER;
                    pos++;
                    return node;
                case '?':
                    node.op = Operator.OP_QUESTION;
                    pos++;
                    return node;
                case ':':
                    node.op = Operator.OP_COLON;
                    pos++;
                    return node;
                case '!':
                    if (pos + 1 < toParse[line].Length)
                    {
                        if (toParse[line][pos + 1] == '=')
                        {
                            if (pos + 2 < toParse[line].Length && toParse[line][pos + 2] == '=')
                            {
                                node.op = Operator.OP_NOTSTRICTEQUAL;
                                pos += 3;
                                return node;
                            }
                            node.op = Operator.OP_NOTEQUAL;
                            pos += 2;
                            return node;
                        }
                    }
                    node.op = Operator.OP_NOT;
                    pos++;
                    return node;
                case '~':
                    node.op = Operator.OP_BITNOT;
                    pos++;
                    return node;
                case '#':
                    node.op = Operator.OP_CHAR;
                    pos++;
                    return node;
                case '$':
                    node.op = Operator.OP_DOLLAR;
                    pos++;
                    return node;
                case ';':
                    node.op = Operator.OP_SEMICOLON;
                    pos++;
                    return node;
                case '.':
                    if (pos + 1 < toParse[line].Length && char.IsDigit(toParse[line][pos + 1]))
                    {
                        node.variable = new TJSVariable(ParseNumber());
                        node.op = Operator.OP_CONST;
                        return node;
                    }
                    node.op = Operator.OP_DOT;
                    nextIsBareWord = true;
                    pos++;
                    return node;
                case '(':
                    node.op = Operator.OP_PARENTHESES;
                    pos++;
                    return node;
                case ')':
                    node.op = Operator.OP_PARENTHESES2;
                    pos++;
                    return node;
                case '[':
                    node.op = Operator.OP_BRACKET;
                    pos++;
                    return node;
                case ']':
                    node.op = Operator.OP_BRACKET2;
                    pos++;
                    return node;
                case '{':
                    node.op = Operator.OP_BRACE;
                    pos++;
                    return node;
                case '}':
                    node.op = Operator.OP_BRACE2;
                    pos++;
                    return node;
                default:
                    if (char.IsDigit(toParse[line][pos]))
                    {
                        node.variable = new TJSVariable(ParseNumber());
                        node.op = Operator.OP_CONST;
                        return node;
                    }
                    else if (char.IsLetter(toParse[line][pos]) || toParse[line][pos] == '_' || toParse[line][pos] > 127)
                    {
                        int start = pos;
                        pos++;
                        while (pos < toParse[line].Length)
                        {
                            if (char.IsLetterOrDigit(toParse[line][pos]) || toParse[line][pos] == '_')
                            {
                                pos++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        string name = toParse[line].Substring(start, pos - start);
                        if (!nextIsBareWord && operators.ContainsKey(name))
                        {
                            node.op = operators[name];
                            return node;
                        }
                        else
                        {
                            node.op = Operator.OP_LITERAL;
                            node.name = name;
                            return node;
                        }
                    }
                    else if (toParse[line][pos] == '"' || toParse[line][pos] == '\'')
                    {
                        string str = "";
                        char startchar = toParse[line][pos];
                        pos++;
                        while (pos < toParse[line].Length)
                        {
                            if (toParse[line][pos] == '\\')
                            {
                                if (pos + 1 >= toParse[line].Length)
                                {
                                    var e = new TJSException("string not ended");
                                    e.AddTrace(line, pos - 1);
                                    throw e;
                                }
                                switch (toParse[line][pos + 1])
                                {
                                    case '\\':
                                        str += '\\';
                                        break;
                                    case '\'':
                                        str += '\'';
                                        break;
                                    case '\"':
                                        str += '\'';
                                        break;
                                    case 'n':
                                        str += '\n';
                                        break;
                                    case 'r':
                                        str += '\r';
                                        break;
                                    case 't':
                                        str += '\t';
                                        break;
                                    case 'a':
                                        str += '\a';
                                        break;
                                    case 'b':
                                        str += '\b';
                                        break;
                                    case 'f':
                                        str += '\f';
                                        break;
                                    case 'v':
                                        str += '\v';
                                        break;
                                    case 'X':
                                    case 'x':
                                        {
                                            int l = 0;
                                            while (l <= 4 && pos + 1 + l < toParse[line].Length)
                                            {
                                                char cc = toParse[line][pos + 1 + l];
                                                if ((cc >= '0' && cc <= '9') || (cc >= 'A' && cc <= 'F') || (cc >= 'a' && cc <= 'f'))
                                                {
                                                    l++;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                            if (l == 0)
                                            {
                                                var e = new TJSException("illegal \\X sequence");
                                                e.AddTrace(line, pos);
                                                throw e;
                                            }
                                            string xstr = "0X" + toParse[line].Substring(pos + 2, l);
                                            char c = (char)int.Parse(xstr);
                                            str += c;
                                            pos += l;   // then will add 2 out the switch block
                                            break;
                                        }
                                    default:
                                        {
                                            var e = new TJSException("illegal escape sequence");
                                            e.AddTrace(line, pos);
                                            throw e;
                                        }
                                }
                                pos += 2;
                            }
                            else if (toParse[line][pos] == startchar)
                            {
                                break;
                            }
                            else
                            {
                                str += toParse[line][pos];
                                pos++;
                            }
                        }
                        pos++;
                        node.op = Operator.OP_CONST;
                        node.variable = new TJSVariable(str);
                        return node;
                    }
                    else
                    {
                        var e = new TJSException("invalid char");
                        e.AddTrace(line, pos);
                        throw e;
                    }
            }
        }
    }

    struct ILCode
    {
        public VMCode code;
        public int op1;
        public int op2;
        public int op3;

        public int line;
        public int offset;
    }

    class TJSIL
    {
        internal List<TJSVariable> consts = new List<TJSVariable>();
        internal int nagetiveStack;
        internal int positiveStack;
        internal List<ILCode> codes = new List<ILCode>();

        // Dictionary<string, int> localvar;

        internal void AddCode(VMCode c, int p1, int p2, int p3, int l, int p)
        {
            codes.Add(new ILCode { code = c, op1 = p1, op2 = p2, op3 = p3, line = l, offset = p });
        }
    }

    class TJSParser
    {
        TJSLexer lexer = new TJSLexer();

        List<int> LBP = new List<int>();

        HashSet<Operator> head = new HashSet<Operator>();

        List<TJSNode> cache = new List<TJSNode>();

        class CompileClosure
        {
            public Dictionary<string, int> varpos = new Dictionary<string, int>();

            public int nextPos;

            public CompileClosure parent = null;

            public CompileClosure upParent = null;

            public int getPos(string name, out bool IsUpValue)
            {
                IsUpValue = false;
                if (varpos.ContainsKey(name))
                {
                    return varpos[name];
                }
                if (parent != null)
                {
                    return parent.getPos(name, out IsUpValue);
                }
                if (upParent != null)
                {
                    int res = upParent.getPos(name, out IsUpValue);
                    if (res < 0)
                        IsUpValue = true;
                    //upvalue is non-local variable
                    return 0;
                }
                return 0;
            }

            public int RegVar(string name)
            {
                varpos[name] = nextPos;
                nextPos--;
                return nextPos + 1;
            }
        }

        class ContinueBreakClo
        {
            public List<int> continuePos = new List<int>();
            public List<int> breakPos = new List<int>();
        }

        class Compiler
        {
            public CompileClosure closure = null;

            public TJSIL ilcode = new TJSIL();

            public Stack<ContinueBreakClo> cbclo = new Stack<ContinueBreakClo>();

            public void AddNewClosure()
            {
                CompileClosure old = closure;
                closure = new CompileClosure();
                closure.parent = old;
                if (old != null)
                {
                    closure.upParent = old.upParent;
                    closure.nextPos = old.nextPos;
                }
                else
                {
                    closure.upParent = null;
                    closure.nextPos = -1;
                }
            }

            public void RemoveLastClosure()
            {
                int pos = closure.nextPos;
                closure = closure.parent;
                closure.nextPos = pos;
            }
        }

        Stack<Compiler> compilers = new Stack<Compiler>();

        Compiler compiler = null;

        TJSNode peekToken()
        {
            var tk = lexer.ReadToken();
            cache.Add(tk);
            return tk;
        }

        TJSNode readToken()
        {
            if (cache.Count > 0)
            {
                var ret = cache[0];
                cache.RemoveAt(0);
                return ret;
            }
            return lexer.ReadToken();
        }

        public TJSParser()
        {
            LBP.AddRange(Enumerable.Repeat(0, (int)Operator.OP_END + 1));
            head.Add(Operator.OP_IF);
            head.Add(Operator.OP_WHILE);
            head.Add(Operator.OP_DO);
            head.Add(Operator.OP_FOR);
            head.Add(Operator.OP_BRACE);
            head.Add(Operator.OP_CLASS);
            head.Add(Operator.OP_SWITCH);
            head.Add(Operator.OP_DEFAULT);
            head.Add(Operator.OP_CASE);
            head.Add(Operator.OP_BREAK);
            head.Add(Operator.OP_CONTINUE);
            head.Add(Operator.OP_RETURN);
            head.Add(Operator.OP_VAR);

            LBP[(int)Operator.OP_END] = 0;
            LBP[(int)Operator.OP_COLON] = 0;
            LBP[(int)Operator.OP_SEMICOLON] = 0;
            LBP[(int)Operator.OP_PARENTHESES2] = 0;
            LBP[(int)Operator.OP_BRACKET2] = 0;
            LBP[(int)Operator.OP_BRACE2] = 0;
            LBP[(int)Operator.OP_COMMA] = 10;
            LBP[(int)Operator.OP_VALUE] = 10;
            LBP[(int)Operator.OP_SET] = 20;
            LBP[(int)Operator.OP_SETADD] = 20;
            LBP[(int)Operator.OP_SETSUB] = 20;
            LBP[(int)Operator.OP_SETMOD] = 20;
            LBP[(int)Operator.OP_SETMUL] = 20;
            LBP[(int)Operator.OP_SETDIV] = 20;
            LBP[(int)Operator.OP_SETINTDIV] = 20;
            LBP[(int)Operator.OP_SETAND] = 20;
            LBP[(int)Operator.OP_SETOR] = 20;
            LBP[(int)Operator.OP_SETBITAND] = 20;
            LBP[(int)Operator.OP_SETBITOR] = 20;
            LBP[(int)Operator.OP_SETBITXOR] = 20;
            LBP[(int)Operator.OP_SETLEFTSHIFT] = 20;
            LBP[(int)Operator.OP_SETRIGHTSHIFT] = 20;
            LBP[(int)Operator.OP_QUESTION] = 30;
            LBP[(int)Operator.OP_INSTANCEOF] = 40;
            LBP[(int)Operator.OP_OR] = 50;
            LBP[(int)Operator.OP_AND] = 60;
            LBP[(int)Operator.OP_BITOR] = 70;
            LBP[(int)Operator.OP_BITXOR] = 80;
            LBP[(int)Operator.OP_BITAND] = 90;
            LBP[(int)Operator.OP_EQUAL] = 100;
            LBP[(int)Operator.OP_STRICTEQUAL] = 100;
            LBP[(int)Operator.OP_NOTEQUAL] = 100;
            LBP[(int)Operator.OP_NOTSTRICTEQUAL] = 100;
            LBP[(int)Operator.OP_LARGER] = 110;
            LBP[(int)Operator.OP_LARGEREQUAL] = 110;
            LBP[(int)Operator.OP_SMALLER] = 110;
            LBP[(int)Operator.OP_SMALLEREQUAL] = 110;
            LBP[(int)Operator.OP_LEFTSHIFT] = 120;
            LBP[(int)Operator.OP_RIGHTSHIFT] = 120;
            LBP[(int)Operator.OP_ADD] = 130;
            LBP[(int)Operator.OP_SUB] = 130;
            LBP[(int)Operator.OP_MUL] = 140;
            LBP[(int)Operator.OP_MOD] = 140;
            LBP[(int)Operator.OP_DIV] = 140;
            LBP[(int)Operator.OP_INTDIV] = 140;

            LBP[(int)Operator.OP_INC] = 190;
            LBP[(int)Operator.OP_DEC] = 190;
            LBP[(int)Operator.OP_BRACKET] = 200;
            LBP[(int)Operator.OP_DOT] = 200;
            LBP[(int)Operator.OP_PARENTHESES] = 200;
        }

        TJSNode token;
        TJSNode next;

        void _parse(int rbp, int dest, bool canBeNull = false)
        {
            if (compiler.ilcode.positiveStack < dest + 5)
                compiler.ilcode.positiveStack = dest + 5;
            token = next;
            next = readToken();
            if (rbp >= 5 && head.Contains(token.op))
            {
                var e = new TJSException("this token must be at line head");
                e.AddTrace(token.line, token.pos);
                throw e;
            }
            var res = token;
            switch (token.op)
            {
                case Operator.OP_END:
                case Operator.OP_SEMICOLON:
                    if (!canBeNull)
                    {
                        var e = new TJSException("unexpected sentence end");
                        e.AddTrace(next.line, next.pos);
                        throw e;
                    }
                    return;
                case Operator.OP_IF: //if(exp)exp;else exp   
                    {
                        if (next.op != Operator.OP_PARENTHESES)
                        {
                            var e = new TJSException("need (");
                            e.AddTrace(next.line, next.pos);
                            throw e;
                        }
                        compiler.AddNewClosure();
                        try
                        {
                            _parse(300, dest);
                            int jumpaddress = compiler.ilcode.codes.Count;
                            compiler.ilcode.AddCode(VMCode.VM_JUMPFALSE, dest, 0, 0, token.line, token.pos);
                            _parse(0, dest, true);
                            if (next.op == Operator.OP_END)
                            {
                                var c = compiler.ilcode.codes[jumpaddress];
                                c.op2 = compiler.ilcode.codes.Count;
                                compiler.ilcode.codes[jumpaddress] = c;
                                compiler.RemoveLastClosure();
                                return;
                            }
                            if (next.op == Operator.OP_BRACE2 || next.op == Operator.OP_SEMICOLON)
                            {
                                if (peekToken().op == Operator.OP_ELSE)
                                {
                                    readToken();    //read else
                                    next = readToken();
                                    int jumpaddress2 = jumpaddress = compiler.ilcode.codes.Count;
                                    compiler.ilcode.AddCode(VMCode.VM_JUMP, 0, 0, 0, token.line, token.pos);
                                    var c = compiler.ilcode.codes[jumpaddress];
                                    c.op2 = compiler.ilcode.codes.Count;
                                    compiler.ilcode.codes[jumpaddress] = c;
                                    _parse(0, dest, true);
                                    c = compiler.ilcode.codes[jumpaddress2];
                                    c.op1 = compiler.ilcode.codes.Count;
                                    compiler.ilcode.codes[jumpaddress2] = c;
                                    compiler.RemoveLastClosure();
                                    return;
                                }
                            }
                            else
                            {
                                var e = new TJSException("invalid token");
                                e.AddTrace(next.line, next.pos);
                                throw e;
                            }
                        }
                        catch (TJSException e)
                        {
                            compiler.RemoveLastClosure();
                            throw e;
                        }
                    }
                    return;
                case Operator.OP_WHILE: //while(exp)exp
                    {
                        if (next.op != Operator.OP_PARENTHESES)
                        {
                            var e = new TJSException("need (");
                            e.AddTrace(next.line, next.pos);
                            throw e;
                        }
                        compiler.AddNewClosure();
                        ContinueBreakClo newcbclo = null;
                        try
                        {
                            int jumptarget = compiler.ilcode.codes.Count;
                            _parse(300, dest);
                            int jumpaddress = compiler.ilcode.codes.Count;
                            compiler.ilcode.AddCode(VMCode.VM_JUMPFALSE, dest, 0, 0, token.line, token.pos);
                            newcbclo = new ContinueBreakClo();
                            compiler.cbclo.Push(newcbclo);
                            _parse(0, dest, true);
                            compiler.ilcode.AddCode(VMCode.VM_JUMP, jumptarget, 0, 0, token.line, token.pos);
                            int count = compiler.ilcode.codes.Count;
                            var c = compiler.ilcode.codes[jumpaddress];
                            c.op2 = count;
                            compiler.ilcode.codes[jumpaddress] = c;
                            foreach (var it in compiler.cbclo.Peek().breakPos)
                            {
                                c = compiler.ilcode.codes[it];
                                c.op1 = count;
                                compiler.ilcode.codes[it] = c;
                            }
                            foreach (var it in compiler.cbclo.Peek().continuePos)
                            {
                                c = compiler.ilcode.codes[it];
                                c.op1 = jumptarget;
                                compiler.ilcode.codes[it] = c;
                            }
                        }
                        catch (TJSException e)
                        {
                            if (newcbclo != null)
                                compiler.cbclo.Pop();
                            compiler.RemoveLastClosure();
                            throw e;
                        }
                        compiler.cbclo.Pop();
                        compiler.RemoveLastClosure();
                    }
                    return;
                case Operator.OP_DO:    //do exp while(exp)
                    {
                        int jumptarget = compiler.ilcode.codes.Count;
                        next = readToken();
                        compiler.AddNewClosure();
                        compiler.cbclo.Push(new ContinueBreakClo());
                        try
                        {
                            _parse(0, dest, true);
                        }
                        catch (TJSException e)
                        {
                            compiler.cbclo.Pop();
                            compiler.RemoveLastClosure();
                            throw e;
                        }
                        if (next.op == Operator.OP_BRACE2 || next.op == Operator.OP_SEMICOLON)
                        {
                            next = readToken();
                            if (next.op == Operator.OP_WHILE)
                            {
                                next = readToken();
                                if (next.op == Operator.OP_PARENTHESES)
                                {
                                    int count = compiler.ilcode.codes.Count;
                                    ContinueBreakClo cbclo = compiler.cbclo.Peek();
                                    compiler.cbclo.Pop();
                                    foreach (var it in cbclo.continuePos)
                                    {
                                        var c = compiler.ilcode.codes[it];
                                        c.op1 = jumptarget;
                                        compiler.ilcode.codes[it] = c;
                                    }
                                    try
                                    {
                                        _parse(300, dest);
                                    }
                                    catch (TJSException e)
                                    {
                                        compiler.RemoveLastClosure();
                                        throw e;
                                    }
                                    compiler.ilcode.AddCode(VMCode.VM_JUMPTRUE, dest, jumptarget, 0, token.line, token.pos);
                                    count = compiler.ilcode.codes.Count;
                                    foreach (var it in cbclo.breakPos)
                                    {
                                        var c = compiler.ilcode.codes[it];
                                        c.op1 = count;
                                        compiler.ilcode.codes[it] = c;
                                    }
                                    compiler.RemoveLastClosure();
                                    return;
                                }
                                else
                                {
                                    var e = new TJSException("need (");
                                    e.AddTrace(next.line, next.pos);
                                    throw e;
                                }
                            }
                            else
                            {
                                var e = new TJSException("need while");
                                e.AddTrace(next.line, next.pos);
                                throw e;
                            }
                        }
                        else if (next.op == Operator.OP_END)
                        {
                            var e = new TJSException("unexpected end");
                            e.AddTrace(next.line, next.pos);
                            throw e;
                        }
                        else
                        {
                            var e = new TJSException("invalid token");
                            e.AddTrace(next.line, next.pos);
                            throw e;
                        }
                    }
                case Operator.OP_FOR:   //for(exp;exp;exp)exp
                    {
                        if (next.op != Operator.OP_PARENTHESES)
                        {
                            var e = new TJSException("need (");
                            e.AddTrace(next.line, next.pos);
                            throw e;
                        }
                        next = readToken();
                        compiler.AddNewClosure();
                        ContinueBreakClo newcbclo = null;
                        try
                        {
                            _parse(5, dest, true);
                            if (next.op != Operator.OP_SEMICOLON)
                            {
                                var e = new TJSException("need ;");
                                e.AddTrace(next.line, next.pos);
                                throw e;
                            }
                            next = readToken();
                            int jumptarget = compiler.ilcode.codes.Count;
                            int jumpaddress1 = -1;
                            if (next.op != Operator.OP_SEMICOLON)
                            {
                                _parse(5, dest, false);
                                jumpaddress1 = compiler.ilcode.codes.Count;
                                compiler.ilcode.AddCode(VMCode.VM_JUMPFALSE, dest, 0, 0, token.line, token.pos);
                            }
                            int jumpaddress2 = compiler.ilcode.codes.Count;
                            compiler.ilcode.AddCode(VMCode.VM_JUMP, 0, 0, 0, token.line, token.pos);
                            if (next.op != Operator.OP_SEMICOLON)
                            {
                                var e = new TJSException("need ;");
                                e.AddTrace(next.line, next.pos);
                                throw e;
                            }
                            next = readToken();
                            _parse(5, dest, true);
                            compiler.ilcode.AddCode(VMCode.VM_JUMP, jumptarget, 0, 0, token.line, token.pos);
                            var c = compiler.ilcode.codes[jumpaddress2];
                            c.op1 = compiler.ilcode.codes.Count;
                            compiler.ilcode.codes[jumpaddress2] = c;
                            if (next.op != Operator.OP_PARENTHESES2)
                            {
                                var e = new TJSException("need )");
                                e.AddTrace(next.line, next.pos);
                                throw e;
                            }
                            next = readToken();
                            newcbclo = new ContinueBreakClo();
                            compiler.cbclo.Push(newcbclo);
                            _parse(0, dest, true);
                            compiler.ilcode.AddCode(VMCode.VM_JUMP, jumpaddress2 + 1, 0, 0, token.line, token.pos);
                            int count = compiler.ilcode.codes.Count;
                            if (jumpaddress1 > -1)
                            {
                                c = compiler.ilcode.codes[jumpaddress1];
                                c.op2 = count;
                                compiler.ilcode.codes[jumpaddress1] = c;
                            }
                            foreach (var it in compiler.cbclo.Peek().breakPos)
                            {
                                c = compiler.ilcode.codes[it];
                                c.op1 = count;
                                compiler.ilcode.codes[it] = c;
                            }
                            foreach (var it in compiler.cbclo.Peek().continuePos)
                            {
                                c = compiler.ilcode.codes[it];
                                c.op1 = jumpaddress2 + 1;
                                compiler.ilcode.codes[it] = c;
                            }
                            compiler.cbclo.Pop();
                            compiler.RemoveLastClosure();
                        }
                        catch (TJSException e)
                        {
                            if (newcbclo != null)
                                compiler.cbclo.Pop();
                            compiler.RemoveLastClosure();
                            throw e;
                        }
                    }
                    return;
                case Operator.OP_CONTINUE:
                    if (compiler.cbclo.Count == 0)
                    {
                        var e = new TJSException("no context for continue");
                        e.AddTrace(token.line, token.pos);
                        throw e;
                    }
                    compiler.cbclo.Peek().continuePos.Add(compiler.ilcode.codes.Count);
                    compiler.ilcode.AddCode(VMCode.VM_JUMP, 0, 0, 0, token.line, token.pos);
                    return;
                case Operator.OP_BREAK:
                    if (compiler.cbclo.Count == 0)
                    {
                        var e = new TJSException("no context for break");
                        e.AddTrace(token.line, token.pos);
                        throw e;
                    }
                    compiler.cbclo.Peek().breakPos.Add(compiler.ilcode.codes.Count);
                    compiler.ilcode.AddCode(VMCode.VM_JUMP, 0, 0, 0, token.line, token.pos);
                    return;
                case Operator.OP_VAR:   //var literal = exp, literal = exp
                    {
                        string varname;
                        int varpos = 0;
                        while (true)
                        {
                            if (next.op != Operator.OP_LITERAL)
                            {
                                var e = new TJSException("need variable name");
                                e.AddTrace(next.line, next.pos);
                                throw e;
                            }
                            varname = next.name;
                            compiler.ilcode.consts.Add(new TJSVariable(varname));
                            if (compiler.closure != null)
                            {
                                varpos = compiler.closure.RegVar(varname);
                                compiler.ilcode.AddCode(VMCode.VM_REGVAR, varpos, compiler.ilcode.consts.Count - 1, 0, token.line, token.pos);
                            }
                            next = readToken();
                            if (next.op == Operator.OP_SET)
                            {
                                next = readToken();
                                _parse(LBP[(int)Operator.OP_COMMA], dest);
                                if (compiler.closure != null)
                                {
                                    compiler.ilcode.AddCode(VMCode.VM_COPY, varpos, dest, 0, token.line, token.pos);
                                }
                                else
                                {
                                    compiler.ilcode.AddCode(VMCode.VM_LOADCONST, dest + 1, compiler.ilcode.consts.Count - 1, 0, token.line, token.pos);
                                    compiler.ilcode.AddCode(VMCode.VM_DOTSET, 0, dest + 1, dest, token.line, token.pos);
                                }
                            }
                            else
                            {
                                if (compiler.closure != null)
                                {
                                    compiler.ilcode.AddCode(VMCode.VM_LOADVOID, varpos, 0, 0, token.line, token.pos);
                                }
                                else
                                {
                                    compiler.ilcode.AddCode(VMCode.VM_LOADCONST, dest, compiler.ilcode.consts.Count - 1, 0, token.line, token.pos);
                                    compiler.ilcode.AddCode(VMCode.VM_DOTSETVOID, 0, dest, 0, token.line, token.pos);
                                }
                            }
                            if (next.op == Operator.OP_COMMA)
                            {
                                next = readToken();
                                continue;
                            }
                            else if (next.op == Operator.OP_SEMICOLON)
                            {
                                break;
                            }
                            else
                            {
                                var e = new TJSException("invalid token");
                                e.AddTrace(next.line, next.pos);
                                throw e;
                            }
                        }
                    }
                    return;
                case Operator.OP_RETURN:
                    if (next.op == Operator.OP_END || next.op == Operator.OP_SEMICOLON)
                    {
                        compiler.ilcode.AddCode(VMCode.VM_RETURNVOID, 0, 0, 0, token.line, token.pos);
                    }
                    else
                    {
                        _parse(5, dest, false);
                        compiler.ilcode.AddCode(VMCode.VM_RETURN, dest, 0, 0, token.line, token.pos);
                    }
                    return;
                case Operator.OP_DELETE:
                    {
                        _parse(5, dest);
                        var c = compiler.ilcode.codes[compiler.ilcode.codes.Count - 1];
                        if (c.code == VMCode.VM_COPY)
                        {
                            //a local var
                            c.code = VMCode.VM_LOADVOID;
                            c.op2 = 0;
                            compiler.ilcode.codes[compiler.ilcode.codes.Count - 1] = c;
                        }
                        else if (c.code == VMCode.VM_DOT)
                        {
                            //global var or member
                            c.code = VMCode.VM_DELETE;
                            c.op1 = c.op2;
                            c.op2 = c.op3;
                            c.op3 = 0;
                            compiler.ilcode.codes[compiler.ilcode.codes.Count - 1] = c;
                        }
                        else
                        {
                            var e = new TJSException("cannot delete non-variable");
                            e.AddTrace(next.line, next.pos);
                            throw e;
                        }
                    }
                    return;
                case Operator.OP_BRACE:
                    compiler.AddNewClosure();
                    try
                    {
                        while (next.op != Operator.OP_BRACE2)
                        {
                            if (next.op == Operator.OP_BRACE)
                            {
                                _parse(0, dest, true);
                                next = readToken();
                            }
                            else
                            {
                                _parse(0, dest, true);
                                if (next.op == Operator.OP_SEMICOLON)
                                {
                                    next = readToken();
                                }
                                else if (next.op == Operator.OP_END)
                                {
                                    var e = new TJSException("unexpected end");
                                    e.AddTrace(next.line, next.pos);
                                    throw e;
                                }
                                else
                                {
                                    var e = new TJSException("invalid token, should be ; or }");
                                    e.AddTrace(next.line, next.pos);
                                    throw e;
                                }
                            }
                        }
                        compiler.RemoveLastClosure();
                        if (peekToken().op == Operator.OP_SEMICOLON)
                            next = readToken();
                    }
                    catch (TJSException e)
                    {
                        throw e;
                    }
                    return;
                case Operator.OP_SWITCH:
                case Operator.OP_CASE:
                case Operator.OP_DEFAULT:
                case Operator.OP_CLASS:
                    //TODO
                    return;
                case Operator.OP_FUNCTION:  //function name(param=exp,...)
                    {
                        string name = null;
                        if (next.op == Operator.OP_LITERAL)
                        {
                            name = next.name;
                            next = readToken();
                        }
                        else if (rbp < 5)
                        {
                            var e = new TJSException("defining a function must give a name");
                            e.AddTrace(next.line, next.pos);
                            throw e;
                        }
                        if (next.op != Operator.OP_PARENTHESES)
                        {
                            var e = new TJSException("need (");
                            e.AddTrace(next.line, next.pos);
                            throw e;
                        }
                        var newcp = new Compiler();
                        newcp.AddNewClosure();
                        newcp.closure.upParent = compiler.closure;
                        int dest2 = dest + 1;
                        bool hasDefault = false;
                        HashSet<string> varnames = new HashSet<string>();
                        try
                        {
                            next = readToken();
                            if (next.op != Operator.OP_PARENTHESES2)
                            {
                                while (true)
                                {
                                    if (next.op != Operator.OP_LITERAL)
                                    {
                                        var e = new TJSException("need a variable name");
                                        e.AddTrace(next.line, next.pos);
                                        throw e;
                                    }
                                    if(varnames.Contains(next.name))
                                    {
                                        var e = new TJSException("duplicate parameter name");
                                        e.AddTrace(next.line, next.pos);
                                        throw e;
                                    }
                                    int target = newcp.closure.RegVar(next.name);
                                    varnames.Add(next.name);
                                    newcp.ilcode.AddCode(VMCode.VM_REGVAR, target, newcp.ilcode.consts.Count, 0, next.line, next.pos);
                                    newcp.ilcode.consts.Add(new TJSVariable(next.name));
                                    next = readToken();
                                    if (next.op == Operator.OP_SET)
                                    {
                                        hasDefault = true;
                                        next = readToken();
                                        _parse(LBP[(int)Operator.OP_COMMA], dest2);
                                    }
                                    else
                                    {
                                        if (hasDefault)
                                        {
                                            var e = new TJSException("default parameters must be placed last");
                                            e.AddTrace(next.line, next.pos);
                                            throw e;
                                        }
                                        compiler.ilcode.AddCode(VMCode.VM_LOADVOID, dest2, 0, 0, token.line, token.pos);
                                    }
                                    dest2++;
                                    if (next.op == Operator.OP_PARENTHESES2)
                                    {
                                        break;
                                    }
                                    if (next.op != Operator.OP_COMMA)
                                    {
                                        var e = new TJSException("need a variable name");
                                        e.AddTrace(next.line, next.pos);
                                        throw e;
                                    }
                                    next = readToken();
                                }
                            }
                            next = readToken();
                            if (next.op != Operator.OP_BRACE)
                            {
                                var e = new TJSException("need {");
                                e.AddTrace(next.line, next.pos);
                                throw e;
                            }
                        }
                        catch (TJSException e)
                        {
                            throw e;
                        }
                        try
                        {
                            compilers.Push(newcp);
                            compiler = newcp;
                            _parse(0, 2);
                        }
                        catch (TJSException e)
                        {
                            compilers.Pop();
                            compiler = compilers.Peek();
                            throw e;
                        }
                        newcp.ilcode.nagetiveStack = newcp.closure.nextPos + 1;
                        var func = new TJSFunction(newcp.ilcode, null);
                        compilers.Pop();
                        compiler = compilers.Peek();
                        compiler.ilcode.AddCode(VMCode.VM_LOADCONST, dest, compiler.ilcode.consts.Count, 0, token.line, token.pos);
                        compiler.ilcode.consts.Add(new TJSVariable(func));
                        compiler.ilcode.AddCode(VMCode.VM_MAKEFUNC, dest, dest + 1, dest2, token.line, token.pos);
                        if (name != null)
                        {
                            if (compiler.closure == null)
                            {
                                compiler.ilcode.AddCode(VMCode.VM_LOADCONST, dest + 1, compiler.ilcode.consts.Count, 0, token.line, token.pos);
                                compiler.ilcode.consts.Add(new TJSVariable(name));
                                compiler.ilcode.AddCode(VMCode.VM_DOTSET, 0, dest + 1, dest, token.line, token.pos);
                            }
                            else
                            {
                                int pos = compiler.closure.RegVar(name);
                                compiler.ilcode.AddCode(VMCode.VM_SETFUNCCLOSURE, dest, 0, 0, token.line, token.pos);
                                compiler.ilcode.AddCode(VMCode.VM_COPY, pos, dest, 0, token.line, token.pos);
                            }
                        }
                        else if(compiler.closure != null)
                        {
                            compiler.ilcode.AddCode(VMCode.VM_SETFUNCCLOSURE, dest, 0, 0, token.line, token.pos);
                        }
                        if (rbp >= 5 && next.op == Operator.OP_BRACE2)
                        {
                            next = readToken();
                        }
                    }
                    break;
                case Operator.OP_PARENTHESES:
                    if (next.op == Operator.OP_INT || next.op == Operator.OP_REAL || next.op == Operator.OP_STRING)
                    {
                        if (peekToken().op == Operator.OP_PARENTHESES2)
                        {
                            readToken();
                            _parse(300, dest);
                            return;
                        }
                    }
                    _parse(5, dest);
                    if (next.op != Operator.OP_PARENTHESES2)
                    {
                        var e = new TJSException("need )");
                        e.AddTrace(next.line, next.pos);
                        throw e;
                    }
                    next = readToken();
                    break;
                case Operator.OP_BRACKET:   //[exp,exp,...]
                    {
                        int dest2 = dest;
                        if (next.op != Operator.OP_BRACKET2)
                        {
                            while (true)
                            {
                                if (next.op == Operator.OP_COMMA || next.op == Operator.OP_BRACKET2)
                                    compiler.ilcode.AddCode(VMCode.VM_LOADVOID, dest2, 0, 0, next.line, next.pos);
                                else
                                    _parse(LBP[(int)Operator.OP_COMMA], dest2);
                                dest2++;
                                if (next.op == Operator.OP_COMMA)
                                {
                                    next = readToken();
                                }
                                else if (next.op == Operator.OP_BRACKET2)
                                {
                                    break;
                                }
                                else
                                {
                                    var e = new TJSException("invalid token");
                                    e.AddTrace(next.line, next.pos);
                                    throw e;
                                }
                            }
                        }
                        next = readToken();
                        compiler.ilcode.AddCode(VMCode.VM_MAKEARRAY, dest, dest2, 0, token.line, token.pos);
                    }
                    break;
                case Operator.OP_DIC:   //%[literal:exp, exp=>exp]
                    {
                        int dest2 = dest;
                        while (next.op != Operator.OP_BRACKET2)
                        {
                            if (next.op == Operator.OP_LITERAL && peekToken().op == Operator.OP_COLON)
                            {
                                compiler.ilcode.AddCode(VMCode.VM_LOADCONST, dest2, compiler.ilcode.consts.Count, 0, next.line, next.pos);
                                compiler.ilcode.consts.Add(new TJSVariable(next.name));
                                readToken();
                                next = readToken();
                                _parse(LBP[(int)Operator.OP_COMMA], dest2 + 1);
                                dest2 += 2;
                            }
                            else
                            {
                                _parse(LBP[(int)Operator.OP_VALUE], dest2);
                                if (next.op == Operator.OP_VALUE)
                                {
                                    next = readToken();
                                }
                                else if (next.op != Operator.OP_BRACKET2)
                                {
                                    var e = new TJSException("need =>");
                                    e.AddTrace(next.line, next.pos);
                                    throw e;
                                }
                                _parse(LBP[(int)Operator.OP_COMMA], dest2 + 1);
                                dest2 += 2;
                            }
                            if (next.op == Operator.OP_COMMA)
                            {
                                next = readToken();
                            }
                            else if (next.op != Operator.OP_BRACKET2)
                            {
                                var e = new TJSException("invalid token");
                                e.AddTrace(next.line, next.pos);
                                throw e;
                            }
                        }
                        next = readToken();
                        compiler.ilcode.AddCode(VMCode.VM_MAKEDIC, dest, dest2, 0, token.line, token.pos);
                    }
                    break;
                case Operator.OP_ADD:
                    _parse(LBP[(int)Operator.OP_INC], dest);
                    compiler.ilcode.AddCode(VMCode.VM_TOINT, dest, 0, 0, token.line, token.pos);
                    return;
                case Operator.OP_SUB:
                    _parse(LBP[(int)Operator.OP_INC], dest);
                    compiler.ilcode.AddCode(VMCode.VM_TOMINUSNUM, dest, 0, 0, token.line, token.pos);
                    return;
                case Operator.OP_INC:
                case Operator.OP_DEC:
                    {
                        _parse(LBP[(int)Operator.OP_INC], dest);
                        var c = compiler.ilcode.codes[compiler.ilcode.codes.Count - 1];
                        if (c.code == VMCode.VM_COPY)
                        {
                            //local var
                            if (token.op == Operator.OP_INC)
                                compiler.ilcode.AddCode(VMCode.VM_INC, c.op1, c.op2, 0, token.line, token.pos);
                            else
                                compiler.ilcode.AddCode(VMCode.VM_DEC, c.op1, c.op2, 0, token.line, token.pos);
                            compiler.ilcode.codes.RemoveAt(compiler.ilcode.codes.Count - 2);
                        }
                        else if (c.code == VMCode.VM_DOT)
                        {
                            //global var or member
                            while (c.op1 == c.op2 || c.op1 == c.op3)
                            {
                                c.op1++;
                            }
                            compiler.ilcode.codes[compiler.ilcode.codes.Count - 1] = c;
                            if (token.op == Operator.OP_INC)
                                compiler.ilcode.AddCode(VMCode.VM_INC, c.op1, c.op1, 0, token.line, token.pos);
                            else
                                compiler.ilcode.AddCode(VMCode.VM_DEC, c.op1, c.op1, 0, token.line, token.pos);
                            compiler.ilcode.AddCode(VMCode.VM_DOTSET, c.op2, c.op3, c.op1, token.line, token.pos);
                            if (c.op1 != dest)
                            {
                                compiler.ilcode.AddCode(VMCode.VM_COPY, dest, c.op1, 0, token.line, token.pos);
                            }
                        }
                        else
                        {
                            var e = new TJSException("operand must be variable");
                            e.AddTrace(token.line, token.pos);
                            throw e;
                        }
                    }
                    break;
                case Operator.OP_NOT:
                    _parse(LBP[(int)Operator.OP_INC] - 1, dest);
                    compiler.ilcode.AddCode(VMCode.VM_NOT, dest, dest, 0, token.line, token.pos);
                    break;
                case Operator.OP_BITNOT:
                    _parse(LBP[(int)Operator.OP_INC] - 1, dest);
                    compiler.ilcode.AddCode(VMCode.VM_BITNOT, dest, dest, 0, token.line, token.pos);
                    break;
                case Operator.OP_LITERAL:
                    {
                        int varpos = 0;
                        bool isUpValue = false;
                        if (compiler.closure != null)
                            varpos = compiler.closure.getPos(token.name, out isUpValue);
                        if (varpos < 0)
                        {
                            //local var
                            compiler.ilcode.AddCode(VMCode.VM_COPY, dest, varpos, 0, token.line, token.pos);
                        }
                        else
                        {
                            if (isUpValue)
                                compiler.ilcode.AddCode(VMCode.VM_REGUPVALUE, compiler.ilcode.consts.Count, 0, 0, token.line, token.pos);
                            compiler.ilcode.AddCode(VMCode.VM_LOADCONST, dest, compiler.ilcode.consts.Count, 0, token.line, token.pos);
                            compiler.ilcode.consts.Add(new TJSVariable(token.name));
                            compiler.ilcode.AddCode(VMCode.VM_DOT, dest, 0, dest, token.line, token.pos);
                        }
                    }
                    break;
                case Operator.OP_TRUE:
                    compiler.ilcode.AddCode(VMCode.VM_LOADTRUE, dest, 0, 0, token.line, token.pos);
                    break;
                case Operator.OP_FALSE:
                    compiler.ilcode.AddCode(VMCode.VM_LOADFALSE, dest, 0, 0, token.line, token.pos);
                    break;
                case Operator.OP_CONST:
                    compiler.ilcode.AddCode(VMCode.VM_LOADCONST, dest, compiler.ilcode.consts.Count, 0, token.line, token.pos);
                    compiler.ilcode.consts.Add(token.variable);
                    break;
                case Operator.OP_GLOBAL:
                    compiler.ilcode.AddCode(VMCode.VM_GLOBAL, dest, 0, 0, token.line, token.pos);
                    break;
                case Operator.OP_THIS:
                    compiler.ilcode.AddCode(VMCode.VM_COPY, dest, 1, 0, token.line, token.pos);
                    break;
                case Operator.OP_SUPER:
                    if (next.op != Operator.OP_DOT && next.op != Operator.OP_BRACKET)
                    {
                        var e = new TJSException("super cannot be used with . or [ to get member");
                        e.AddTrace(token.line, token.pos);
                        throw e;
                    }
                    break;
                case Operator.OP_TYPEOF:
                    _parse(LBP[(int)Operator.OP_BRACKET] - 5, dest);
                    compiler.ilcode.AddCode(VMCode.VM_TYPEOF, dest, dest, 0, token.line, token.pos);
                    break;
                case Operator.OP_NEW:
                    _parse(300, dest);
                    compiler.ilcode.AddCode(VMCode.VM_NEW, dest, dest, 0, token.line, token.pos);
                    break;
                case Operator.OP_CHAR:
                    _parse(LBP[(int)Operator.OP_BRACKET] - 5, dest);
                    compiler.ilcode.AddCode(VMCode.VM_CHAR, dest, dest, 0, token.line, token.pos);
                    break;
                case Operator.OP_DOLLAR:
                    _parse(LBP[(int)Operator.OP_BRACKET] - 5, dest);
                    compiler.ilcode.AddCode(VMCode.VM_STR, dest, dest, 0, token.line, token.pos);
                    break;
                case Operator.OP_INT:
                    _parse(LBP[(int)Operator.OP_BRACKET] - 5, dest);
                    compiler.ilcode.AddCode(VMCode.VM_TOINT, dest, dest, 0, token.line, token.pos);
                    break;
                case Operator.OP_REAL:
                    _parse(LBP[(int)Operator.OP_BRACKET] - 5, dest);
                    compiler.ilcode.AddCode(VMCode.VM_TONUMBER, dest, dest, 0, token.line, token.pos);
                    break;
                case Operator.OP_STRING:
                    _parse(LBP[(int)Operator.OP_BRACKET] - 5, dest);
                    compiler.ilcode.AddCode(VMCode.VM_TOSTRING, dest, dest, 0, token.line, token.pos);
                    break;
                default:
                    {
                        var e = new TJSException("invalid token");
                        e.AddTrace(token.line, token.pos);
                        throw e;
                    }
            }
            while (LBP[(int)next.op] > rbp)
            {
                token = next;
                next = readToken();
                switch (token.op)
                {
                    case Operator.OP_COMMA:
                        _parse(5, dest);
                        break;
                    case Operator.OP_SET:
                    case Operator.OP_SETBITAND:
                    case Operator.OP_SETBITOR:
                    case Operator.OP_SETBITXOR:
                    case Operator.OP_SETSUB:
                    case Operator.OP_SETADD:
                    case Operator.OP_SETMOD:
                    case Operator.OP_SETDIV:
                    case Operator.OP_SETINTDIV:
                    case Operator.OP_SETMUL:
                    case Operator.OP_SETAND:
                    case Operator.OP_SETOR:
                    case Operator.OP_SETLEFTSHIFT:
                    case Operator.OP_SETRIGHTSHIFT:
                        //right combine
                        {
                            var op = token.op;
                            var cpos = compiler.ilcode.codes.Count - 1;
                            var c = compiler.ilcode.codes[cpos];
                            if (c.code != VMCode.VM_COPY && c.code != VMCode.VM_DOT)
                            {
                                var e = new TJSException("operand must be variable");
                                e.AddTrace(token.line, token.pos);
                                throw e;
                            }
                            if (c.code == VMCode.VM_COPY)
                            {
                                //local var
                                _parse(LBP[(int)token.op] - 1, dest);
                                if (op == Operator.OP_SET)
                                {
                                    compiler.ilcode.AddCode(VMCode.VM_COPY, c.op2, dest, 0, token.line, token.pos);
                                }
                                else
                                {
                                    compiler.ilcode.AddCode(token.op - Operator.OP_SETBITAND + VMCode.VM_BITAND, c.op2, c.op2, dest, token.line, token.pos);
                                    compiler.ilcode.AddCode(VMCode.VM_COPY, dest, c.op2, 0, token.line, token.pos);
                                }
                                compiler.ilcode.codes.RemoveAt(cpos);
                            }
                            else if (c.code == VMCode.VM_DOT)
                            {
                                //global var or member
                                while (c.op1 <= c.op2 || c.op1 <= c.op3)
                                {
                                    c.op1++;
                                }
                                compiler.ilcode.codes[compiler.ilcode.codes.Count - 1] = c;
                                _parse(LBP[(int)token.op] - 1, c.op1 + 1);
                                if (op == Operator.OP_SET)
                                {
                                    compiler.ilcode.AddCode(VMCode.VM_DOTSET, c.op2, c.op3, c.op1 + 1, token.line, token.pos);
                                    if (c.op1 + 1 != dest)
                                        compiler.ilcode.AddCode(VMCode.VM_COPY, dest, c.op1 + 1, 0, token.line, token.pos);
                                    compiler.ilcode.codes.RemoveAt(cpos);
                                }
                                else
                                {
                                    compiler.ilcode.AddCode(token.op - Operator.OP_SETBITAND + VMCode.VM_BITAND, c.op1, c.op1, c.op1 + 1, token.line, token.pos);
                                    compiler.ilcode.AddCode(VMCode.VM_DOTSET, c.op2, c.op3, c.op1, token.line, token.pos);
                                    if (c.op1 != dest)
                                        compiler.ilcode.AddCode(VMCode.VM_COPY, dest, c.op1, 0, token.line, token.pos);
                                }
                            }
                            else
                            {
                                var e = new TJSException("operand must be variable");
                                e.AddTrace(token.line, token.pos);
                                throw e;
                            }
                        }
                        break;
                    case Operator.OP_BITAND:
                    case Operator.OP_BITOR:
                    case Operator.OP_SUB:
                    case Operator.OP_ADD:
                    case Operator.OP_MOD:
                    case Operator.OP_DIV:
                    case Operator.OP_INTDIV:
                    case Operator.OP_MUL:
                    case Operator.OP_AND:
                    case Operator.OP_OR:
                    case Operator.OP_LEFTSHIFT:
                    case Operator.OP_RIGHTSHIFT:
                    case Operator.OP_EQUAL:
                    case Operator.OP_STRICTEQUAL:
                    case Operator.OP_NOTEQUAL:
                    case Operator.OP_NOTSTRICTEQUAL:
                    case Operator.OP_LARGER:
                    case Operator.OP_LARGEREQUAL:
                    case Operator.OP_SMALLER:
                    case Operator.OP_SMALLEREQUAL:
                    case Operator.OP_BITXOR:
                        {
                            var op = token.op;
                            _parse(LBP[(int)token.op], dest + 1);
                            compiler.ilcode.AddCode(op - Operator.OP_BITAND + VMCode.VM_BITAND, dest, dest, dest + 1, token.line, token.pos);
                        }
                        break;
                    case Operator.OP_INSTANCEOF:
                        _parse(LBP[(int)token.op], dest + 1);
                        compiler.ilcode.AddCode(VMCode.VM_INSTANCEOF, dest, dest, dest + 1, token.line, token.pos);
                        break;
                    case Operator.OP_PARENTHESES:
                        {
                            int dest2 = dest + 1;
                            if (next.op != Operator.OP_PARENTHESES2)
                            {
                                while (true)
                                {
                                    if (next.op == Operator.OP_COMMA || next.op == Operator.OP_PARENTHESES2)
                                        compiler.ilcode.AddCode(VMCode.VM_LOADVOID, dest2, 0, 0, next.line, next.pos);
                                    else
                                        _parse(LBP[(int)Operator.OP_COMMA], dest2);
                                    dest2++;
                                    if (next.op == Operator.OP_COMMA)
                                    {
                                        next = readToken();
                                    }
                                    else if (next.op == Operator.OP_PARENTHESES2)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        var e = new TJSException("invalid token");
                                        e.AddTrace(next.line, next.pos);
                                        throw e;
                                    }
                                }
                            }
                            next = readToken();
                            compiler.ilcode.AddCode(VMCode.VM_CALL, dest, dest + 1, dest2, token.line, token.pos);
                        }
                        break;
                    case Operator.OP_BRACKET:
                        _parse(LBP[(int)token.op], dest + 1);
                        if (next.op != Operator.OP_BRACKET2)
                        {
                            var e = new TJSException("need ]");
                            e.AddTrace(next.line, next.pos);
                            throw e;
                        }
                        next = readToken();
                        compiler.ilcode.AddCode(VMCode.VM_DOT, dest, dest, dest + 1, token.line, token.pos);
                        break;
                    case Operator.OP_DOT:
                        if (next.op != Operator.OP_LITERAL)
                        {
                            var e = new TJSException("need a variable name");
                            e.AddTrace(next.line, next.pos);
                            throw e;
                        }
                        compiler.ilcode.AddCode(VMCode.VM_LOADCONST, dest, compiler.ilcode.consts.Count, 0, token.line, token.pos);
                        compiler.ilcode.consts.Add(new TJSVariable(next.name));
                        compiler.ilcode.AddCode(VMCode.VM_DOT, dest, dest, dest + 1, token.line, token.pos);
                        next = readToken();
                        break;
                    case Operator.OP_INC:
                    case Operator.OP_DEC:
                        {
                            var op = token.op;
                            var c = compiler.ilcode.codes[compiler.ilcode.codes.Count - 1];
                            if (c.code == VMCode.VM_COPY)
                            {
                                //local var
                                if (op == Operator.OP_INC)
                                    compiler.ilcode.AddCode(VMCode.VM_POSTINC, c.op1, c.op2, 0, token.line, token.pos);
                                else
                                    compiler.ilcode.AddCode(VMCode.VM_POSTDEC, c.op1, c.op2, 0, token.line, token.pos);
                                compiler.ilcode.codes.RemoveAt(compiler.ilcode.codes.Count - 2);
                            }
                            else if (c.code == VMCode.VM_DOT)
                            {
                                //global var or member
                                while (c.op1 == c.op2 || c.op1 == c.op3)
                                {
                                    c.op1++;
                                }
                                compiler.ilcode.codes[compiler.ilcode.codes.Count - 1] = c;
                                if (op == Operator.OP_INC)
                                    compiler.ilcode.AddCode(VMCode.VM_POSTINC, c.op1 + 1, c.op1, 0, token.line, token.pos);
                                else
                                    compiler.ilcode.AddCode(VMCode.VM_POSTDEC, c.op1 + 1, c.op1, 0, token.line, token.pos);
                                compiler.ilcode.AddCode(VMCode.VM_DOTSET, c.op2, c.op3, c.op1, token.line, token.pos);
                                if (c.op1 + 1 != dest)
                                {
                                    compiler.ilcode.AddCode(VMCode.VM_COPY, dest, c.op1 + 1, 0, token.line, token.pos);
                                }
                            }
                            else
                            {
                                var e = new TJSException("operand must be variable");
                                e.AddTrace(token.line, token.pos);
                                throw e;
                            }
                        }
                        break;
                }
            }
            return;
        }

        public TJSIL Parse(string exp)
        {
            compilers.Clear();
            compiler = new Compiler();
            compilers.Push(compiler);
            cache.Clear();
            lexer.LoadScript(exp);
            next = readToken();
            while (next.op != Operator.OP_END)
            {
                _parse(0, 2, true);
                if (next.op == Operator.OP_SEMICOLON || next.op == Operator.OP_BRACE2)
                    next = readToken();
            }
            if (compiler.closure == null)
                compiler.ilcode.nagetiveStack = 0;
            else
                compiler.ilcode.nagetiveStack = compiler.closure.nextPos + 1;
            return compiler.ilcode;
        }
    }

    public struct TJSVariable
    {
        internal double num;
        internal string str;
        internal TJSObject obj;

        internal VarType vt;

        /// <summary>
        /// build a void or undefined variable
        /// </summary>
        /// <param name="dummy">void or undefined</param>
        internal TJSVariable(VarType dummy)
        {
            vt = dummy;
            num = 0;
            str = null;
            obj = null;
        }

        public TJSVariable(int v)
        {
            num = v;
            str = null;
            obj = null;
            vt = VarType.NUMBER;
        }

        public TJSVariable(bool v)
        {
            num = v ? 1 : 0;
            str = null;
            obj = null;
            vt = VarType.NUMBER;
        }

        public TJSVariable(double v)
        {
            num = v;
            str = null;
            obj = null;
            vt = VarType.NUMBER;
        }

        public TJSVariable(float v)
        {
            num = v;
            str = null;
            obj = null;
            vt = VarType.NUMBER;
        }

        public TJSVariable(string v)
        {
            num = 0;
            str = v;
            obj = null;
            vt = VarType.STRING;
        }

        public TJSVariable(List<TJSVariable> v)
        {
            num = 0;
            str = null;
            obj = new TJSArray(v);
            vt = VarType.ARRAY;
        }

        public TJSVariable(Dictionary<string, TJSVariable> v)
        {
            num = 0;
            str = null;
            obj = new TJSDictionary(v);
            vt = VarType.DICTIONARY;
        }

        internal TJSVariable(TJSObject v)
        {
            num = 0;
            str = null;
            obj = v;
            if (v != null)
                vt = v.GetVarType();
            else
                vt = VarType.VOID;
        }

        public bool IsVoid()
        {
            return vt == VarType.VOID;
        }

        public bool IsArray()
        {
            return vt == VarType.ARRAY;
        }

        public bool IsBoolean()
        {
            return vt == VarType.NUMBER;
        }

        public bool IsClass()
        {
            return vt == VarType.CLASS;
        }

        public bool IsDic()
        {
            return vt == VarType.DICTIONARY;
        }

        public bool IsDouble()
        {
            return vt == VarType.NUMBER;
        }

        public bool IsInt()
        {
            return vt == VarType.NUMBER && Math.Abs(num - Math.Round(num)) > 1e-9;
        }

        public bool IsString()
        {
            return vt == VarType.STRING;
        }

        public bool Equal(TJSVariable v)
        {
            if (vt == v.vt)
                return StrictEqual(v);
            if (vt == VarType.VOID || vt == VarType.UNDEFINED)
            {
                if (v.vt == VarType.VOID || v.vt == VarType.UNDEFINED)
                    return true;
                else if (v.vt == VarType.NUMBER)
                    return Math.Abs(v.num) <= 1e-9;
                else if (v.vt == VarType.STRING)
                    return v.str == null || v.str == "";
                else
                    return false;
            }
            else if (vt == VarType.NUMBER)
            {
                if (v.vt == VarType.VOID || v.vt == VarType.UNDEFINED)
                    return Math.Abs(num) <= 1e-9;
                else if (v.vt == VarType.STRING)
                    return num.ToString() == v.str;
                else
                    return false;
            }
            else if (vt == VarType.STRING)
            {
                if (v.vt == VarType.VOID || v.vt == VarType.UNDEFINED)
                    return str == null || str == "";
                else if (v.vt == VarType.NUMBER)
                    return v.num.ToString() == str;
                else
                    return false;
            }
            else
                return false;
        }

        public bool StrictEqual(TJSVariable v)
        {
            if (vt != v.vt)
                return false;
            switch (vt)
            {
                case VarType.VOID:
                    return true;
                case VarType.NUMBER:
                    return Math.Abs(num - v.num) <= 1e-9;
                case VarType.STRING:
                    return str == v.str;
                case VarType.ARRAY:
                case VarType.DICTIONARY:
                case VarType.FUNCTION:
                case VarType.CLASS:
                case VarType.CLOSURE:
                    return obj == v.obj;
                case VarType.UNDEFINED:
                    return true;
                default:
                    return false;
            }
        }

        public TJSVariable RunAsFunc()
        {
            throw new NotImplementedException();
        }

        public List<TJSVariable> ToArray()
        {
            if (vt == VarType.ARRAY)
            {
                return ((TJSArray)obj).arr;
            }
            throw new TJSException("cannot convert to array");
        }

        public bool ToBoolean()
        {
            switch (vt)
            {
                case VarType.NUMBER:
                    return Math.Abs(num) > 1e-9;
                case VarType.STRING:
                    if (str == "false" || str == null || str == "")
                        return false;
                    return true;
                case VarType.UNDEFINED:
                case VarType.VOID:
                    return false;
                default:
                    return true;
            }
        }

        public Dictionary<string, TJSVariable> ToDic()
        {
            if (vt == VarType.DICTIONARY)
            {
                return ((TJSDictionary)obj).dic;
            }
            throw new TJSException("cannot convert to dictionary");
        }

        public double ToDouble()
        {
            switch (vt)
            {
                case VarType.NUMBER:
                    return num;
                case VarType.STRING:
                    {
                        double v = 0;
                        double.TryParse(str, out v);
                        return v;
                    }
                case VarType.UNDEFINED:
                case VarType.VOID:
                    return 0;
                default:
                    throw new TJSException("cannot convert to double");
            }
        }

        public int ToInt()
        {
            switch (vt)
            {
                case VarType.NUMBER:
                    return (int)num;
                case VarType.STRING:
                    {
                        int v = 0;
                        int.TryParse(str, out v);
                        return v;
                    }
                case VarType.UNDEFINED:
                case VarType.VOID:
                    return 0;
                default:
                    throw new TJSException("cannot convert to int");
            }
        }

        public override string ToString()
        {
            switch (vt)
            {
                case VarType.NUMBER:
                    return num.ToString();
                case VarType.STRING:
                    if (str == null)
                        str = "";
                    return str;
                case VarType.UNDEFINED:
                case VarType.VOID:
                    return "";
                default:
                    return obj.ConvertToString();
            }
        }

        public TJSVariable Dot(TJSVariable name)
        {
            switch (vt)
            {
                case VarType.VOID:
                    throw new TJSException("cannot get member of void");
                case VarType.NUMBER:
                    if (name.IsInt())
                        return TJSVM.VM.numClass.GetField(name.ToInt());
                    else
                        return TJSVM.VM.numClass.GetField(name.ToString());
                case VarType.STRING:
                    if (name.IsInt())
                        return TJSVM.VM.strClass.GetField(name.ToInt());
                    else
                        return TJSVM.VM.strClass.GetField(name.ToString());
                case VarType.ARRAY:
                    if (name.IsInt())
                        return TJSVM.VM.arrClass.GetField(name.ToInt());
                    else
                        return TJSVM.VM.arrClass.GetField(name.ToString());
                case VarType.DICTIONARY:
                    if (name.IsInt())
                        return TJSVM.VM.dicClass.GetField(name.ToInt());
                    else
                        return TJSVM.VM.dicClass.GetField(name.ToString());
                case VarType.FUNCTION:
                    throw new TJSException("cannot get member of function");
                case VarType.CLASS:
                case VarType.CLOSURE:
                    if (name.IsInt())
                        return obj.GetField(name.ToInt());
                    else
                        return obj.GetField(name.ToString());
                case VarType.UNDEFINED:
                    throw new TJSException("cannot get member of undefined");
                default:
                    throw new TJSException("internal type error");
            }
        }

        public void DotSet(TJSVariable name, TJSVariable value)
        {
            switch (vt)
            {
                case VarType.VOID:
                    throw new TJSException("cannot set member of void");
                case VarType.NUMBER:
                    if (name.IsInt())
                        TJSVM.VM.numClass.SetField(name.ToInt(), value);
                    else
                        TJSVM.VM.numClass.SetField(name.ToString(), value);
                    break;
                case VarType.STRING:
                    if (name.IsInt())
                        TJSVM.VM.strClass.SetField(name.ToInt(), value);
                    else
                        TJSVM.VM.strClass.SetField(name.ToString(), value);
                    break;
                case VarType.ARRAY:
                    if (name.IsInt())
                        TJSVM.VM.arrClass.SetField(name.ToInt(), value);
                    else
                        TJSVM.VM.arrClass.SetField(name.ToString(), value);
                    break;
                case VarType.DICTIONARY:
                    if (name.IsInt())
                        TJSVM.VM.dicClass.SetField(name.ToInt(), value);
                    else
                        TJSVM.VM.dicClass.SetField(name.ToString(), value);
                    break;
                case VarType.FUNCTION:
                    throw new TJSException("cannot get member of function");
                case VarType.CLASS:
                case VarType.CLOSURE:
                    if (name.IsInt())
                        obj.SetField(name.ToInt(), value);
                    else
                        obj.SetField(name.ToString(), value);
                    break;
                case VarType.UNDEFINED:
                    throw new TJSException("cannot get member of undefined");
                default:
                    throw new TJSException("internal type error");
            }
        }

        public void Remove(TJSVariable index)
        {
            switch (vt)
            {
                case VarType.VOID:
                    throw new TJSException("cannot set member of void");
                case VarType.NUMBER:
                    if (index.IsInt())
                        TJSVM.VM.numClass.RemoveField(index.ToInt());
                    else
                        TJSVM.VM.numClass.RemoveField(index.ToString());
                    break;
                case VarType.STRING:
                    if (index.IsInt())
                        TJSVM.VM.strClass.RemoveField(index.ToInt());
                    else
                        TJSVM.VM.strClass.RemoveField(index.ToString());
                    break;
                case VarType.ARRAY:
                    if (index.IsInt())
                        TJSVM.VM.arrClass.RemoveField(index.ToInt());
                    else
                        TJSVM.VM.arrClass.RemoveField(index.ToString());
                    break;
                case VarType.DICTIONARY:
                    if (index.IsInt())
                        TJSVM.VM.dicClass.RemoveField(index.ToInt());
                    else
                        TJSVM.VM.dicClass.RemoveField(index.ToString());
                    break;
                case VarType.FUNCTION:
                    throw new TJSException("cannot get member of function");
                case VarType.CLASS:
                case VarType.CLOSURE:
                    if (index.IsInt())
                        obj.RemoveField(index.ToInt());
                    else
                        obj.RemoveField(index.ToString());
                    break;
                case VarType.UNDEFINED:
                    throw new TJSException("cannot get member of undefined");
                default:
                    throw new TJSException("internal type error");
            }
        }

        public string GetTypeString()
        {
            switch (vt)
            {
                case VarType.VOID:
                    return "void";
                case VarType.NUMBER:
                    return "number";
                case VarType.STRING:
                    return "string";
                case VarType.ARRAY:
                case VarType.DICTIONARY:
                case VarType.FUNCTION:
                case VarType.CLASS:
                case VarType.CLOSURE:
                    return "object";
                case VarType.UNDEFINED:
                    return "undefined";
                default:
                    throw new TJSException("internal type error");
            }
        }

        public bool IsInstanceOf(string name)
        {
            switch (vt)
            {
                case VarType.VOID:
                    return false;
                case VarType.NUMBER:
                    return name == "number";
                case VarType.STRING:
                    return name == "string";
                case VarType.ARRAY:
                    return name == "array" || name == "object";
                case VarType.DICTIONARY:
                    return name == "dictionary" || name == "object";
                case VarType.FUNCTION:
                    return name == "function" || name == "object";
                case VarType.CLASS:
                    return ((TJSClass)obj).IsInctanceOf(name);
                case VarType.CLOSURE:
                    return name == "closure" || name == "object";
                case VarType.UNDEFINED:
                    return false;
                default:
                    throw new TJSException("internal type error");
            }
        }
    }

    /// <summary>
    /// Only a subset of TJS syntax
    /// </summary>
    public class TJSVM
    {
        static TJSVM _VM = null;

        TJSParser parser = new TJSParser();

        public TJSClosure _global = new TJSNormalClosure(null);

        public TJSClass numClass = new TJSClass();
        public TJSClass strClass = new TJSClass();
        public TJSClass arrClass = new TJSClass();
        public TJSClass dicClass = new TJSClass();

        private TJSVM()
        {
            _global.SetField("int", new TJSVariable(numClass));
            _global.SetField("string", new TJSVariable(strClass));
            _global.SetField("array", new TJSVariable(arrClass));
            _global.SetField("dictionary", new TJSVariable(dicClass));

            _global.SetField("log", new TJSVariable(new TJSFunction(Log, new TJSVariable(VarType.VOID))));
        }

        private static TJSVariable Log(TJSVariable _this, TJSVariable[] param, int start, int length)
        {
            string str = "";
            for (int i = 0; i < length; i++)
            {
                if (i != 0)
                    str += ',';
                str += param[i + start].ToString();
            }
            UnityEngine.Debug.Log(str);
            return new TJSVariable(VarType.VOID);
        }

        public static TJSVM VM
        {
            get
            {
                if (_VM == null)
                    _VM = new TJSVM();
                return _VM;
            }
        }

        internal TJSVariable Run(TJSIL code, TJSClosure closure, TJSVariable _this, TJSVariable[] param = null)
        {
            var stackclosure = new TJSStackClosure(closure);
            var neg = -code.nagetiveStack;
            stackclosure.localStack = new TJSVariable[code.positiveStack + neg];
            var stack = stackclosure.localStack;
            stack[0 + neg] = new TJSVariable(stackclosure);
            stack[1 + neg] = _this;
            if (param != null)
            {
                for (int i = 0; i < param.Length; i++)
                {
                    stack[neg - i - 1] = param[i];
                }
            }
            int start = 0;
            while (start < code.codes.Count)
            {
                try
                {
                    var c = code.codes[start];
                    switch (c.code)
                    {
                        case VMCode.VM_BITAND:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToInt() & stack[c.op3 + neg].ToInt());
                            start++;
                            break;
                        case VMCode.VM_BITOR:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToInt() | stack[c.op3 + neg].ToInt());
                            start++;
                            break;
                        case VMCode.VM_BITNOT:
                            stack[c.op1 + neg] = new TJSVariable(~stack[c.op2 + neg].ToInt());
                            start++;
                            break;
                        case VMCode.VM_SUB:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToDouble() - stack[c.op3 + neg].ToDouble());
                            start++;
                            break;
                        case VMCode.VM_ADD:
                            if (stack[c.op2 + neg].IsString())
                                stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToString() + stack[c.op3 + neg].ToString());
                            else
                                stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToDouble() + stack[c.op3 + neg].ToDouble());
                            start++;
                            break;
                        case VMCode.VM_MOD:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToDouble() % stack[c.op3 + neg].ToDouble());
                            start++;
                            break;
                        case VMCode.VM_DIV:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToDouble() / stack[c.op3 + neg].ToDouble());
                            start++;
                            break;
                        case VMCode.VM_INTDIV:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToInt() / stack[c.op3 + neg].ToInt());
                            start++;
                            break;
                        case VMCode.VM_MUL:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToDouble() * stack[c.op3 + neg].ToDouble());
                            start++;
                            break;
                        case VMCode.VM_AND:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToBoolean() && stack[c.op3 + neg].ToBoolean());
                            start++;
                            break;
                        case VMCode.VM_OR:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToBoolean() || stack[c.op3 + neg].ToBoolean());
                            start++;
                            break;
                        case VMCode.VM_LEFTSHIFT:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToInt() << stack[c.op3 + neg].ToInt());
                            start++;
                            break;
                        case VMCode.VM_RIGHTSHIFT:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToInt() >> stack[c.op3 + neg].ToInt());
                            start++;
                            break;
                        case VMCode.VM_EQUAL:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].Equal(stack[c.op3 + neg]));
                            start++;
                            break;
                        case VMCode.VM_STRICTEQUAL:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].StrictEqual(stack[c.op3 + neg]));
                            start++;
                            break;
                        case VMCode.VM_NOTEQUAL:
                            stack[c.op1 + neg] = new TJSVariable(!stack[c.op2 + neg].Equal(stack[c.op3 + neg]));
                            start++;
                            break;
                        case VMCode.VM_NOTSTRICTEQUAL:
                            stack[c.op1 + neg] = new TJSVariable(!stack[c.op2 + neg].StrictEqual(stack[c.op3 + neg]));
                            start++;
                            break;
                        case VMCode.VM_LARGER:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToDouble() > stack[c.op3 + neg].ToDouble());
                            start++;
                            break;
                        case VMCode.VM_LARGEREQUAL:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToDouble() >= stack[c.op3 + neg].ToDouble());
                            start++;
                            break;
                        case VMCode.VM_SMALLER:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToDouble() < stack[c.op3 + neg].ToDouble());
                            start++;
                            break;
                        case VMCode.VM_SMALLEREQUAL:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToDouble() <= stack[c.op3 + neg].ToDouble());
                            start++;
                            break;
                        case VMCode.VM_BITXOR:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToInt() ^ stack[c.op3 + neg].ToInt());
                            start++;
                            break;
                        case VMCode.VM_NOT:
                            stack[c.op1 + neg] = new TJSVariable(!stack[c.op2 + neg].ToBoolean());
                            start++;
                            break;
                        case VMCode.VM_DOT:
                            stack[c.op1 + neg] = stack[c.op2 + neg].Dot(stack[c.op3 + neg]);
                            start++;
                            break;
                        case VMCode.VM_DOTSET:
                            stack[c.op1 + neg].DotSet(stack[c.op2 + neg], stack[c.op3 + neg]);
                            start++;
                            break;
                        case VMCode.VM_DOTSETVOID:
                            stack[c.op1 + neg].DotSet(stack[c.op2 + neg], new TJSVariable(VarType.VOID));
                            start++;
                            break;
                        case VMCode.VM_LOADCONST:
                            stack[c.op1 + neg] = code.consts[c.op2];
                            start++;
                            break;
                        case VMCode.VM_COPY:
                            stack[c.op1 + neg] = stack[c.op2 + neg];
                            start++;
                            break;
                        case VMCode.VM_LOADVOID:
                            stack[c.op1 + neg] = new TJSVariable(VarType.VOID);
                            start++;
                            break;
                        case VMCode.VM_LOADTRUE:
                            stack[c.op1 + neg] = new TJSVariable(true);
                            start++;
                            break;
                        case VMCode.VM_LOADFALSE:
                            stack[c.op1 + neg] = new TJSVariable(false);
                            start++;
                            break;
                        case VMCode.VM_CALL:
                            {
                                if (stack[c.op1 + neg].vt != VarType.FUNCTION)
                                {
                                    throw new TJSException("the object be called must be a function");
                                }
                                var func = (TJSFunction)stack[c.op1 + neg].obj;
                                stack[c.op1 + neg] = func.CallAsFunc(stack, c.op2 + neg, c.op3 - c.op2);
                            }
                            start++;
                            break;
                        case VMCode.VM_JUMP:
                            start = c.op1;
                            break;
                        case VMCode.VM_RETURN:
                            return stack[c.op1 + neg];
                        case VMCode.VM_RETURNVOID:
                            return new TJSVariable(VarType.VOID);
                        case VMCode.VM_JUMPFALSE:
                            if (!stack[c.op1 + neg].ToBoolean())
                                start = c.op2;
                            start++;
                            break;
                        case VMCode.VM_JUMPTRUE:
                            if (stack[c.op1 + neg].ToBoolean())
                                start = c.op2;
                            start++;
                            break;
                        case VMCode.VM_TOINT:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToInt());
                            start++;
                            break;
                        case VMCode.VM_TOMINUSNUM:
                            stack[c.op1 + neg] = new TJSVariable(-stack[c.op2 + neg].ToDouble());
                            start++;
                            break;
                        case VMCode.VM_TOSTRING:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToString());
                            start++;
                            break;
                        case VMCode.VM_TONUMBER:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToDouble());
                            start++;
                            break;
                        case VMCode.VM_INC:
                            stack[c.op2 + neg] = new TJSVariable(stack[c.op2 + neg].ToInt() + 1);
                            if (c.op1 != c.op2)
                                stack[c.op1 + neg] = stack[c.op2 + neg];
                            start++;
                            break;
                        case VMCode.VM_DEC:
                            stack[c.op2 + neg] = new TJSVariable(stack[c.op2 + neg].ToInt() - 1);
                            if (c.op1 != c.op2)
                                stack[c.op1 + neg] = stack[c.op2 + neg];
                            start++;
                            break;
                        case VMCode.VM_POSTINC:
                            stack[c.op1 + neg] = stack[c.op2 + neg];
                            stack[c.op2 + neg] = new TJSVariable(stack[c.op2 + neg].ToInt() + 1);
                            start++;
                            break;
                        case VMCode.VM_POSTDEC:
                            stack[c.op1 + neg] = stack[c.op2 + neg];
                            stack[c.op2 + neg] = new TJSVariable(stack[c.op2 + neg].ToInt() - 1);
                            start++;
                            break;
                        case VMCode.VM_CHAR:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].ToString()[0]);
                            start++;
                            break;
                        case VMCode.VM_STR:
                            {
                                int ch = stack[c.op2 + neg].ToInt();
                                string str = "";
                                str += char.ConvertFromUtf32(ch);
                                stack[c.op1 + neg] = new TJSVariable(str);
                            }
                            start++;
                            break;
                        case VMCode.VM_MAKEARRAY:
                            {
                                //set capacity a little bit more than actual size
                                List<TJSVariable> v = new List<TJSVariable>(c.op2 - c.op1 + 5);
                                for (int s = c.op1 + neg; s < c.op2 + neg; s++)
                                {
                                    v.Add(stack[s]);
                                }
                                stack[c.op1] = new TJSVariable(v);
                            }
                            start++;
                            break;
                        case VMCode.VM_MAKEDIC:
                            {
                                Dictionary<string, TJSVariable> dic = new Dictionary<string, TJSVariable>();
                                for (int s = c.op1 + neg; s < c.op2 + neg; s += 2)
                                {
                                    dic[stack[s].ToString()] = stack[s + 1];
                                }
                                stack[c.op1] = new TJSVariable(dic);
                            }
                            start++;
                            break;
                        case VMCode.VM_MAKEFUNC:
                            {
                                var func = new TJSFunction((TJSFunction)stack[c.op1 + neg].obj);
                                if(c.op3 > c.op2)
                                    func.defaultParam = new TJSVariable[c.op3 - c.op2];
                                else
                                    func.defaultParam = null;
                                for (int i = 0; i < c.op3 - c.op2; i++)
                                {
                                    func.defaultParam[i] = stack[c.op2 + neg + i];
                                }
                                func.closure = VM._global;
                                stack[c.op1 + neg] = new TJSVariable(func);
                            }
                            start++;
                            break;
                        case VMCode.VM_SETFUNCCLOSURE:
                            {
                                var func = (TJSFunction)stack[c.op1 + neg].obj;
                                func.closure = (TJSStackClosure)stack[0 + neg].obj;
                            }
                            start++;
                            break;
                        case VMCode.VM_NEW:
                            //TODO
                            break;
                        case VMCode.VM_TYPEOF:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].GetTypeString());
                            start++;
                            break;
                        case VMCode.VM_DELETE:
                            stack[c.op1 + neg].Remove(stack[c.op2 + neg]);
                            stack[c.op1 + neg] = new TJSVariable(VarType.VOID);
                            start++;
                            break;
                        case VMCode.VM_INSTANCEOF:
                            stack[c.op1 + neg] = new TJSVariable(stack[c.op2 + neg].IsInstanceOf(stack[c.op3].ToString()));
                            start++;
                            break;
                        case VMCode.VM_GLOBAL:
                            stack[c.op1 + neg] = new TJSVariable(VM._global);
                            start++;
                            break;
                        case VMCode.VM_SUPERDOT:
                            //TODO
                            break;
                        case VMCode.VM_REGVAR:
                            {
                                var clo = (TJSStackClosure)stack[0 + neg].obj;
                                if (clo.localvar == null)
                                    clo.localvar = new Dictionary<string, int>();
                                clo.localvar[code.consts[c.op2].ToString()] = c.op1 + neg;
                            }
                            start++;
                            break;
                        case VMCode.VM_REGUPVALUE:
                            ((TJSStackClosure)stack[0 + neg].obj).RegUpValue(code.consts[c.op1].ToString());
                            start++;
                            break;
                        case VMCode.VM_NULL:
                            start++;
                            break;
                        default:
                            throw new TJSException($"unknown VMCode {c.code}");
                    }
                }
                catch (TJSException e)
                {
                    if (start < code.codes.Count)
                        e.AddTrace(code.codes[start].line, code.codes[start].offset);
                    throw e;
                }
            }
            return stack[2 + neg];
        }

        public TJSVariable Eval(string exp)
        {
            TJSIL code = parser.Parse(exp);
            return Run(code, _global, new TJSVariable(VarType.VOID), null);
        }

        public TJSVariable EvalInClosure(string exp, TJSClosure closure)
        {
            TJSIL code = parser.Parse(exp);
            return Run(code, closure, new TJSVariable(VarType.VOID), null);
        }
    }

    class TJSClass4CSharp : TJSClass
    {
        Dictionary<string, TJSVariable> methods = new Dictionary<string, TJSVariable>();

        public TJSClass4CSharp(Type classType, bool hasInstance)
        {
            var methods = classType.GetMethods(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < methods.Length; i++)
            {
                if (makeStaticFunc(methods[i], out TJSVariable v))
                {
                    this.methods[methods[i].Name] = v;
                }
            }
        }

        public override TJSVariable GetField(string name)
        {
            if (methods.ContainsKey(name))
            {
                return methods[name];
            }
            throw new TJSException($"member {name} not exist");
        }

        public override void SetField(string name, TJSVariable value)
        {
            base.SetField(name, value);
        }

        bool makeStaticFunc(MethodInfo m, out TJSVariable func)
        {
            var p = m.GetParameters();
            var r = m.ReturnType;
            if (p.Length == 0)
            {
                func = new TJSVariable(new TJSFunction((TJSVariable _this, TJSVariable[] param, int start, int length) =>
                {
                    var ret = m.Invoke(null, null);
                    if (r == null || r == typeof(void) || ret == null)
                    {
                        return new TJSVariable(VarType.VOID);
                    }
                    if (r == typeof(int))
                    {
                        return new TJSVariable((int)ret);
                    }
                    else if (r == typeof(float))
                    {
                        return new TJSVariable((float)ret);
                    }
                    else if (r == typeof(double))
                    {
                        return new TJSVariable((double)ret);
                    }
                    else if (r == typeof(string))
                    {
                        return new TJSVariable((string)ret);
                    }
                    return new TJSVariable(VarType.VOID);
                }, new TJSVariable(this)));
            }
            func = new TJSVariable();   //unuse
            return false;
        }
    }
}