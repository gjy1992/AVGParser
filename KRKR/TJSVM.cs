using System;
using System.Collections;
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
        PROPERTY,
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
            return ToString() + string.Format(" 0X{0:X8}", GetHashCode());
        }
    }

    public class TJSFunction : TJSObject
    {
        internal TJSClosure closure = null;

        internal TJSVariable _this;

        internal Func<TJSVariable, TJSVariable[], int, int, TJSVariable> nativefunc = null;

        internal TJSIL ilcode = null;

        internal TJSVariable[] defaultParam = null;

        protected TJSFunction()
        {

        }

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
            var v = dic[name];
            if (v.vt == VarType.PROPERTY)
            {
                return ((TJSProperty)v.obj).GetValue();
            }
            return v;
        }

        public override bool hasMember(string name)
        {
            return dic.ContainsKey(name);
        }

        public override void setMember(string name, TJSVariable value)
        {
            if (dic.ContainsKey(name) && dic[name].vt == VarType.PROPERTY)
            {
                ((TJSProperty)dic[name].obj).SetValue(value);
            }
            else
            {
                dic[name] = value;
            }
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
            var v = localStack[localvar[name]];
            if (v.vt == VarType.PROPERTY)
            {
                return ((TJSProperty)v.obj).GetValue();
            }
            return v;
        }

        public override bool hasMember(string name)
        {
            return localvar != null && localvar.ContainsKey(name);
        }

        public override void setMember(string name, TJSVariable value)
        {
            if (localStack[localvar[name]].vt == VarType.PROPERTY)
            {
                ((TJSProperty)localStack[localvar[name]].obj).SetValue(value);
            }
            else
            {
                localStack[localvar[name]] = value;
            }
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

    public class TJSProperty : TJSObject
    {
        internal TJSFunction get;
        internal TJSFunction set;

        public TJSClosure closure
        {
            get
            {
                if (get != null)
                    return get.closure;
                return set.closure;
            }
            set
            {
                if (get != null)
                    get.closure = value;
                if (set != null)
                    set.closure = value;
            }
        }

        public TJSVariable _this
        {
            get
            {
                if (get != null)
                    return get._this;
                return set._this;
            }
            set
            {
                if (get != null)
                    get._this = value;
                if (set != null)
                    set._this = value;
            }
        }

        public TJSProperty(TJSProperty prop)
        {
            if (prop.get != null)
                get = new TJSFunction(prop.get);
            else
                get = null;
            if (prop.set != null)
                set = new TJSFunction(prop.set);
            else
                set = null;
        }

        public TJSProperty(TJSProperty prop, TJSClosure closure, TJSVariable _this)
        {
            if (prop.get != null)
                get = new TJSFunction(prop.get, closure, _this);
            else
                get = null;
            if (prop.set != null)
                set = new TJSFunction(prop.set, closure, _this);
            else
                set = null;
        }

        public TJSProperty(Func<TJSVariable, TJSVariable[], int, int, TJSVariable> getfunc, Func<TJSVariable, TJSVariable[], int, int, TJSVariable> setfunc, TJSVariable _this)
        {
            if (getfunc == null && setfunc == null)
            {
                throw new TJSException("cannot set a null property");
            }
            if (getfunc != null)
            {
                get = new TJSFunction(getfunc, _this);
            }
            if (setfunc != null)
            {
                set = new TJSFunction(setfunc, _this);
            }
        }

        public void addGetFunc(Func<TJSVariable, TJSVariable[], int, int, TJSVariable> getfunc)
        {
            if (getfunc != null)
            {
                get = new TJSFunction(getfunc, _this);
            }
            else
            {
                get = null;
            }
        }

        public void addSetFunc(Func<TJSVariable, TJSVariable[], int, int, TJSVariable> setfunc)
        {
            if (setfunc != null)
            {
                set = new TJSFunction(setfunc, _this);
            }
            else
            {
                set = null;
            }
        }

        public override VarType GetVarType()
        {
            return VarType.PROPERTY;
        }

        public TJSVariable GetValue()
        {
            if (get == null)
                throw new TJSException("this property cannot be read");
            return get.CallAsFunc(null, 0, 0);
        }

        public void SetValue(TJSVariable v)
        {
            if (set == null)
                throw new TJSException("this property cannot be write");
            set.CallAsFunc(new TJSVariable[1] { v }, 0, 1);
        }

        public void SetValue(TJSVariable[] v, int start)
        {
            if (set == null)
                throw new TJSException("this property cannot be write");
            set.CallAsFunc(v, start, 1);
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

        internal TJSClass defineClass = null;

        internal bool isInstance = false;

        internal string name;

        internal Dictionary<string, TJSVariable> defineVars = new Dictionary<string, TJSVariable>();

        internal Dictionary<string, TJSVariable> members = new Dictionary<string, TJSVariable>();

        public bool canInstantiate = false;

        public TJSClass() : base(null)
        {

        }
        public TJSClass(TJSClass def) : base(null)
        {
            defineClass = def;
            isInstance = true;
            foreach (var kv in def.defineVars)
            {
                members.Add(kv.Key, kv.Value);
            }
            canInstantiate = def.canInstantiate;
            name = def.name;
        }

        public virtual TJSVariable CreateInstance(TJSVariable[] param, int start, int length)
        {
            if (!canInstantiate)
            {
                throw new TJSException("该类<" + name + ">不能实例化");
            }
            if (isInstance)
                return new TJSVariable(new TJSClass(defineClass));
            else
                return new TJSVariable(new TJSClass(this));
        }

        public override TJSVariable CallAsFunc(TJSVariable[] param, int start, int length)
        {
            return CreateInstance(param, start, length);
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

        internal bool hasMemberFunc(string name, out TJSVariable v)
        {
            if (members.ContainsKey(name))
            {
                v = members[name];
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
            v = new TJSVariable(VarType.VOID);
            return false;
        }

        public override TJSVariable GetField(int name)
        {
            return GetField(name.ToString());
        }

        public override TJSVariable GetField(string name)
        {
            TJSVariable v;
            if (members.ContainsKey(name))
            {
                v = members[name];
                if (v.vt == VarType.PROPERTY)
                {
                    var prop = (TJSProperty)v.obj;
                    return prop.GetValue();
                }
                return v;
            }
            else if (defineClass != null)
            {
                if (defineClass.hasMemberFunc(name, out v))
                {
                    if (v.vt == VarType.FUNCTION)
                    {
                        v = new TJSVariable(new TJSFunction((TJSFunction)v.obj, this, new TJSVariable(this)));
                        return v;
                    }
                    else if (v.vt == VarType.PROPERTY)
                    {
                        var prop = new TJSProperty((TJSProperty)v.obj, this, new TJSVariable(this));
                        return prop.GetValue();
                    }
                    return v;
                }
            }
            else if (parentClass != null)
            {
                foreach (var p in parentClass)
                {
                    if (p.hasMemberFunc(name, out v))
                    {
                        if (v.vt == VarType.FUNCTION)
                        {
                            v = new TJSVariable(new TJSFunction((TJSFunction)v.obj, this, new TJSVariable(this)));
                            return v;
                        }
                        else if (v.vt == VarType.PROPERTY)
                        {
                            var prop = new TJSProperty((TJSProperty)v.obj, this, new TJSVariable(this));
                            return prop.GetValue();
                        }
                        return v;
                    }
                }
            }
            throw new TJSException($"member {name} not exist");
        }

        public override void SetField(int name, TJSVariable value)
        {
            SetField(name.ToString(), value);
        }

        public virtual bool SetFieldForChildren(string name, TJSVariable value, TJSClass child)
        {
            if(members.ContainsKey(name))
            {
                if (members[name].vt == VarType.PROPERTY)
                {
                    var prop = new TJSProperty((TJSProperty)members[name].obj, child, new TJSVariable(child));
                    prop.SetValue(value);
                }
                return false;
            }
            if (defineClass != null)
            {
                return defineClass.SetFieldForChildren(name, value, child);
            }
            if (parentClass != null)
            {
                foreach (var p in parentClass)
                {
                    if (p.SetFieldForChildren(name, value, child))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void SetField(string name, TJSVariable value)
        {
            bool r = SetFieldForChildren(name, value, this);
            if (!r)
                setMember(name, value);
        }

        public override void setMember(string name, TJSVariable value)
        {
            members[name] = value;
        }

        public override TJSVariable getMember(string name)
        {
            return GetField(name);
        }

        public override bool hasMember(string name)
        {
            return hasMemberFunc(name, out _);
        }

        public override void RemoveField(int name)
        {
            RemoveField(name.ToString());
        }

        public override void RemoveField(string name)
        {
            if (members.ContainsKey(name))
                members.Remove(name);
        }
    }

    class TJSStringContext : TJSObject
    {
        public string str;

        public TJSStringContext(string s) : base()
        {
            this.str = s;
        }

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
                default:
                    {
                        TJSVariable v;
                        if (TJSVM.VM.strClass.hasMemberFunc(name, out v))
                        {
                            if (v.vt == VarType.FUNCTION)
                            {
                                return new TJSVariable(new TJSFunction((TJSFunction)v.obj, null, new TJSVariable(str)));
                            }
                            else if (v.vt == VarType.PROPERTY)
                            {
                                var prop = new TJSProperty((TJSProperty)v.obj, null, new TJSVariable(str));
                                return prop.GetValue();
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

        public TJSNumberContext(double num) : base()
        {
            this.num = num;
        }

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
                            else if (v.vt == VarType.PROPERTY)
                            {
                                var prop = new TJSProperty((TJSProperty)v.obj, null, new TJSVariable(num));
                                return prop.GetValue();
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

        public TJSArrayContext(TJSArray arr) : base()
        {
            this.arr = arr;
        }

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
                return new TJSVariable(VarType.VOID);
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
                            else if (v.vt == VarType.PROPERTY)
                            {
                                var prop = new TJSProperty((TJSProperty)v.obj, null, new TJSVariable(arr));
                                return prop.GetValue();
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

        public TJSDictionaryContext(TJSDictionary dic) : base()
        {
            this.dic = dic;
        }

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
                            else if (v.vt == VarType.PROPERTY)
                            {
                                var prop = new TJSProperty((TJSProperty)v.obj, null, new TJSVariable(dic));
                                return prop.GetValue();
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
                                if (pos + 1 < toParse[line].Length)
                                {
                                    if (toParse[line][pos] == '*' && toParse[line][pos + 1] == '/')
                                    {
                                        pos += 2;
                                        break;
                                    }
                                    pos++;
                                }
                                else
                                {
                                    pos = 0;
                                    line++;
                                }
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
                                    if (varnames.Contains(next.name))
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
                        else if (compiler.closure != null)
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
                    if (next.op == Operator.OP_LITERAL)
                    {
                        string name = next.name;
                        _parse(300, dest);
                        compiler.ilcode.AddCode(VMCode.VM_NEW, dest, dest, compiler.ilcode.consts.Count, token.line, token.pos);
                        compiler.ilcode.consts.Add(new TJSVariable(name));
                    }
                    else
                    {
                        _parse(300, dest);
                        compiler.ilcode.AddCode(VMCode.VM_NEW, dest, dest, -1, token.line, token.pos);
                    }
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
                                var d = compiler.ilcode.codes[compiler.ilcode.codes.Count - 1];
                                if (d.code == VMCode.VM_DOTSET)
                                {
                                    compiler.ilcode.AddCode(VMCode.VM_DOT, dest, c.op2, c.op3, token.line, token.pos);
                                }
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
                                var d = compiler.ilcode.codes[compiler.ilcode.codes.Count - 1];
                                if (d.code == VMCode.VM_DOTSET)
                                {
                                    compiler.ilcode.AddCode(VMCode.VM_DOT, c.op1 + 1, c.op2, c.op3, token.line, token.pos);
                                }
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
                        compiler.ilcode.AddCode(VMCode.VM_LOADCONST, dest + 1, compiler.ilcode.consts.Count, 0, token.line, token.pos);
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

        public bool GreaterThan(TJSVariable v)
        {
            if (vt == VarType.VOID)
                return false;
            if (vt == VarType.NUMBER)
            {
                if (v.vt == VarType.VOID)
                    return true;
                if (v.vt == VarType.NUMBER)
                    return num > v.num;
                if (v.vt == VarType.STRING)
                    return string.Compare(ToString(), v.str) > 0;
                return false;
            }
            if (vt == VarType.STRING)
            {
                if (v.vt == VarType.VOID)
                    return true;
                if (v.vt == VarType.NUMBER)
                    return string.Compare(str, v.ToString()) > 0;
                if (v.vt == VarType.STRING)
                    return string.Compare(str, v.str) > 0;
                return false;
            }
            return false;
        }

        public bool GreaterOrEqual(TJSVariable v)
        {
            if (vt == VarType.VOID)
                return v.vt == VarType.VOID;
            if (vt == VarType.NUMBER)
            {
                if (v.vt == VarType.VOID)
                    return true;
                if (v.vt == VarType.NUMBER)
                    return num >= v.num;
                if (v.vt == VarType.STRING)
                    return string.Compare(ToString(), v.str) >= 0;
                return false;
            }
            if (vt == VarType.STRING)
            {
                if (v.vt == VarType.VOID)
                    return true;
                if (v.vt == VarType.NUMBER)
                    return string.Compare(str, v.ToString()) >= 0;
                if (v.vt == VarType.STRING)
                    return string.Compare(str, v.str) >= 0;
                return false;
            }
            return StrictEqual(v);
        }

        public static bool operator ==(TJSVariable a, TJSVariable b) => a.Equal(b);

        public static bool operator !=(TJSVariable a, TJSVariable b) => !a.Equal(b);

        public static bool operator >(TJSVariable a, TJSVariable b) => a.GreaterThan(b);

        public static bool operator <(TJSVariable a, TJSVariable b) => !a.GreaterThan(b);

        public static bool operator >=(TJSVariable a, TJSVariable b) => a.GreaterOrEqual(b);

        public static bool operator <=(TJSVariable a, TJSVariable b) => !a.GreaterOrEqual(b);

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
                case VarType.CLOSURE:
                    return obj == v.obj;
                case VarType.CLASS:
                    if ((obj is TJSClass4CSharp) && (v.obj is TJSClass4CSharp))
                        return ((TJSClass4CSharp)obj).obj == ((TJSClass4CSharp)v.obj).obj;
                    else
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
                        double v = 0;
                        double.TryParse(str, out v);
                        return (int)v;
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

        public TJSVariable Clone()
        {
            if (vt == VarType.ARRAY)
            {
                var a = new List<TJSVariable>();
                a.AddRange(((TJSArray)obj).arr);
                return new TJSVariable(new TJSArray(a));
            }
            else if (vt == VarType.DICTIONARY)
            {
                var d = new Dictionary<string, TJSVariable>();
                TJSDictionary dic = (TJSDictionary)obj;
                foreach (var kv in dic.dic)
                {
                    d.Add(kv.Key, kv.Value);
                }
                return new TJSVariable(new TJSDictionary(d));
            }
            else
            {
                return this;
            }
        }

        public TJSVariable DeepClone()
        {
            if (vt == VarType.ARRAY)
            {
                var a = new List<TJSVariable>();
                for (int i = 0; i < ((TJSArray)obj).arr.Count; i++)
                {
                    a.Add(((TJSArray)obj).arr[i].DeepClone());
                }
                return new TJSVariable(new TJSArray(a));
            }
            else if (vt == VarType.DICTIONARY)
            {
                var d = new Dictionary<string, TJSVariable>();
                TJSDictionary dic = (TJSDictionary)obj;
                foreach (var kv in dic.dic)
                {
                    d.Add(kv.Key, kv.Value.DeepClone());
                }
                return new TJSVariable(new TJSDictionary(d));
            }
            else
            {
                return this;
            }
        }

        public TJSVariable Dot(TJSVariable name)
        {
            switch (vt)
            {
                case VarType.VOID:
                    throw new TJSException("cannot get member of void");
                case VarType.NUMBER:
                    {
                        var ctx = new TJSNumberContext(num);
                        if (name.IsInt())
                            return ctx.GetField(name.ToInt());
                        else
                            return ctx.GetField(name.ToString());
                    }
                case VarType.STRING:
                    {
                        var ctx = new TJSStringContext(str);
                        if (name.IsInt())
                            return ctx.GetField(name.ToInt());
                        else
                            return ctx.GetField(name.ToString());
                    }
                case VarType.ARRAY:
                    {
                        var ctx = new TJSArrayContext((TJSArray)obj);
                        if (name.IsInt())
                            return ctx.GetField(name.ToInt());
                        else
                            return ctx.GetField(name.ToString());
                    }
                case VarType.DICTIONARY:
                    {
                        var ctx = new TJSDictionaryContext((TJSDictionary)obj);
                        if (name.IsInt())
                            return ctx.GetField(name.ToInt());
                        else
                            return ctx.GetField(name.ToString());
                    }
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
                    {
                        var ctx = new TJSNumberContext(num);
                        if (name.IsInt())
                            ctx.SetField(name.ToInt(), value);
                        else
                            ctx.SetField(name.ToString(), value);
                    }
                    break;
                case VarType.STRING:
                    {
                        var ctx = new TJSStringContext(str);
                        if (name.IsInt())
                            ctx.SetField(name.ToInt(), value);
                        else
                            ctx.SetField(name.ToString(), value);
                    }
                    break;
                case VarType.ARRAY:
                    {
                        var ctx = new TJSArrayContext((TJSArray)obj);
                        if (name.IsInt())
                            ctx.SetField(name.ToInt(), value);
                        else
                            ctx.SetField(name.ToString(), value);
                    }
                    break;
                case VarType.DICTIONARY:
                    {
                        var ctx = new TJSDictionaryContext((TJSDictionary)obj);
                        if (name.IsInt())
                            TJSVM.VM.dicClass.SetField(name.ToInt(), value);
                        else
                            TJSVM.VM.dicClass.SetField(name.ToString(), value);
                    }
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

        public object ConvertToType(Type type)
        {
            if (type == typeof(TJSVariable))
                return this;
            if (TypeUtils.IsNumericTypeAndNullable(type))
            {
                bool nullable = !TypeUtils.IsNumericType(type);
                Type rawType = nullable ? Nullable.GetUnderlyingType(type) : type;
                try
                {
                    double v = ToDouble();
                    return Convert.ChangeType(v, rawType);
                }
                catch (TJSException e)
                {
                    if (nullable)
                        return null;
                    throw e;
                }
            }
            else if (type == typeof(string) || Nullable.GetUnderlyingType(type) == typeof(string))
            {
                return ToString();
            }
            else if (TypeUtils.IsGenericList(type))
            {
                List<TJSVariable> arr;
                try
                {
                    arr = ToArray();
                }
                catch (TJSException)
                {
                    return null;
                }
                if (type == typeof(List<TJSVariable>))
                    return arr;
                var ret = new List<object>();
                var innerType = type.GenericTypeArguments[0];
                foreach (var it in arr)
                {
                    ret.Add(it.ConvertToType(innerType));
                }
                return ret;
            }
            else if (TypeUtils.IsGenericDictionary(type))
            {
                Dictionary<string, TJSVariable> dic;
                try
                {
                    dic = ToDic();
                }
                catch (TJSException)
                {
                    return null;
                }
                if (type == typeof(Dictionary<string, TJSVariable>))
                    return dic;
                var ret = new Dictionary<object, object>();
                var keyType = type.GenericTypeArguments[0];
                var valueType = type.GenericTypeArguments[1];
                foreach (var it in dic)
                {
                    object key = it.Key;
                    if (TypeUtils.IsNumericTypeAndNullable(keyType))
                    {
                        double v;
                        double.TryParse(it.Key, out v);
                        key = Convert.ChangeType(v, keyType);
                    }
                    else if (!(keyType == typeof(string) || Nullable.GetUnderlyingType(keyType) == typeof(string)))
                    {
                        throw new TJSException($"cannot convert key string to type {keyType.ToString()}");
                    }
                    ret[key] = it.Value.ConvertToType(valueType);
                }
                return ret;
            }
            if(IsClass() && (obj is TJSClass4CSharp))
            {
                var c = (TJSClass4CSharp)obj;
                if(c.tp == type)
                {
                    return c.obj;
                }
            }
            throw new TJSException($"cannot convert to type {type.ToString()}");
        }

        public object TryConvertToType(Type type, out bool success)
        {
            success = true;
            if (type == typeof(TJSVariable))
                return this;
            if (TypeUtils.IsNumericTypeAndNullable(type))
            {
                bool nullable = !TypeUtils.IsNumericType(type);
                Type rawType = nullable ? Nullable.GetUnderlyingType(type) : type;
                try
                {
                    double v = ToDouble();
                    return Convert.ChangeType(v, rawType);
                }
                catch (TJSException e)
                {
                    if (nullable)
                        return null;
                    throw e;
                }
            }
            else if (type == typeof(string) || Nullable.GetUnderlyingType(type) == typeof(string))
            {
                return ToString();
            }
            else if (TypeUtils.IsGenericList(type))
            {
                List<TJSVariable> arr;
                try
                {
                    arr = ToArray();
                }
                catch (TJSException)
                {
                    return null;
                }
                if (type == typeof(List<TJSVariable>))
                    return arr;
                var ret = new List<object>();
                var innerType = type.GenericTypeArguments[0];
                foreach (var it in arr)
                {
                    ret.Add(it.ConvertToType(innerType));
                }
                return ret;
            }
            else if (TypeUtils.IsGenericDictionary(type))
            {
                Dictionary<string, TJSVariable> dic;
                try
                {
                    dic = ToDic();
                }
                catch (TJSException)
                {
                    return null;
                }
                if (type == typeof(Dictionary<string, TJSVariable>))
                    return dic;
                var ret = new Dictionary<object, object>();
                var keyType = type.GenericTypeArguments[0];
                var valueType = type.GenericTypeArguments[1];
                foreach (var it in dic)
                {
                    object key = it.Key;
                    if (TypeUtils.IsNumericTypeAndNullable(keyType))
                    {
                        double v;
                        double.TryParse(it.Key, out v);
                        key = Convert.ChangeType(v, keyType);
                    }
                    else if (!(keyType == typeof(string) || Nullable.GetUnderlyingType(keyType) == typeof(string)))
                    {
                        throw new TJSException($"cannot convert key string to type {keyType.ToString()}");
                    }
                    ret[key] = it.Value.ConvertToType(valueType);
                }
                return ret;
            }
            if (IsClass() && (obj is TJSClass4CSharp))
            {
                var c = (TJSClass4CSharp)obj;
                if (c.tp == type)
                {
                    return c.obj;
                }
            }
            success = false;
            return null;
        }
        public object TryConvertToTypeStrict(Type type, out bool success)
        {
            success = true;
            if (type == typeof(TJSVariable))
                return this;
            if (TypeUtils.IsNumericTypeAndNullable(type) && vt == VarType.NUMBER)
            {
                bool nullable = !TypeUtils.IsNumericType(type);
                Type rawType = nullable ? Nullable.GetUnderlyingType(type) : type;
                try
                {
                    double v = ToDouble();
                    return Convert.ChangeType(v, rawType);
                }
                catch (TJSException e)
                {
                    if (nullable)
                        return null;
                    throw e;
                }
            }
            else if ((type == typeof(string) || Nullable.GetUnderlyingType(type) == typeof(string)) && vt == VarType.STRING)
            {
                return ToString();
            }
            else if (TypeUtils.IsGenericList(type))
            {
                List<TJSVariable> arr;
                try
                {
                    arr = ToArray();
                }
                catch (TJSException)
                {
                    return null;
                }
                if (type == typeof(List<TJSVariable>))
                    return arr;
                var ret = new List<object>();
                var innerType = type.GenericTypeArguments[0];
                foreach (var it in arr)
                {
                    ret.Add(it.ConvertToType(innerType));
                }
                return ret;
            }
            else if (TypeUtils.IsGenericDictionary(type))
            {
                Dictionary<string, TJSVariable> dic;
                try
                {
                    dic = ToDic();
                }
                catch (TJSException)
                {
                    return null;
                }
                if (type == typeof(Dictionary<string, TJSVariable>))
                    return dic;
                var ret = new Dictionary<object, object>();
                var keyType = type.GenericTypeArguments[0];
                var valueType = type.GenericTypeArguments[1];
                foreach (var it in dic)
                {
                    object key = it.Key;
                    if (TypeUtils.IsNumericTypeAndNullable(keyType))
                    {
                        double v;
                        double.TryParse(it.Key, out v);
                        key = Convert.ChangeType(v, keyType);
                    }
                    else if (!(keyType == typeof(string) || Nullable.GetUnderlyingType(keyType) == typeof(string)))
                    {
                        throw new TJSException($"cannot convert key string to type {keyType.ToString()}");
                    }
                    ret[key] = it.Value.ConvertToType(valueType);
                }
                return ret;
            }
            if (IsClass() && (obj is TJSClass4CSharp))
            {
                var c = (TJSClass4CSharp)obj;
                if (c.tp == type)
                {
                    return c.obj;
                }
            }
            success = false;
            return null;
        }

        public T ConvertTyType<T>()
        {
            return (T)ConvertToType(typeof(T));
        }

        public static bool IsConvertable(Type type)
        {
            if (type == typeof(TJSVariable))
                return true;
            if (TypeUtils.IsNumericTypeAndNullable(type))
            {
                return true;
            }
            else if (type == typeof(string) || Nullable.GetUnderlyingType(type) == typeof(string))
            {
                return true;
            }
            else if (TypeUtils.IsGenericList(type))
            {
                return IsConvertable(type.GenericTypeArguments[0]);
            }
            else if (TypeUtils.IsGenericDictionary(type))
            {
                var keyType = type.GenericTypeArguments[0];
                var valueType = type.GenericTypeArguments[1];
                if (!(TypeUtils.IsNumericTypeAndNullable(keyType) || type == typeof(string) || Nullable.GetUnderlyingType(type) == typeof(string)))
                {
                    return false;
                }
                return IsConvertable(valueType);
            }
            return false;
        }

        public static TJSVariable ConvertFromType(Type type, object o, bool seeObjectAsCSharp)
        {
            if (o == null)
            {
                return new TJSVariable(VarType.VOID);
            }
            else if (TypeUtils.IsNumericTypeAndNullable(type))
            {
                return new TJSVariable((double)Convert.ChangeType(o, typeof(double)));
            }
            else if (type == typeof(string) || Nullable.GetUnderlyingType(type) == typeof(string))
            {
                return new TJSVariable((string)o);
            }
            else if (!seeObjectAsCSharp && TypeUtils.IsGenericList(type))
            {
                Type innerType = type.GenericTypeArguments[0];
                if (innerType == typeof(TJSVariable))
                    return new TJSVariable((List<TJSVariable>)o);
                var arr = new List<TJSVariable>();
                foreach (var it in (o as IList))
                {
                    arr.Add(ConvertFromType(innerType, it, false));
                }
                return new TJSVariable(arr);
            }
            else if (!seeObjectAsCSharp && TypeUtils.IsGenericDictionary(type))
            {
                var keyType = type.GenericTypeArguments[0];
                var valueType = type.GenericTypeArguments[1];
                if (type == typeof(Dictionary<string, TJSVariable>))
                    return new TJSVariable((Dictionary<string, TJSVariable>)o);
                var dic = new Dictionary<string, TJSVariable>();
                IDictionary d = (IDictionary)o;
                foreach (var it in d.Keys)
                {
                    dic[it.ToString()] = ConvertFromType(valueType, d[it], false);
                }
                return new TJSVariable(dic);
            }
            if(!seeObjectAsCSharp)
                throw new TJSException($"cannot convert from type {type.ToString()}");
            return TJSVM.VM.WrapCSharpObject(o);
        }

        public static TJSVariable ConvertFrom<T>(object o)
        {
            if (o == null)
                return new TJSVariable(VarType.VOID);
            return ConvertFromType(typeof(T), o, false);
        }

        public static TJSVariable ConvertFrom<T>(object o, bool seeObjectAsCSharp)
        {
            if (o == null)
                return new TJSVariable(VarType.VOID);
            return ConvertFromType(typeof(T), o, true);
        }

        public static TJSVariable ConvertFrom(object o)
        {
            if (o == null)
                return new TJSVariable(VarType.VOID);
            return ConvertFromType(o.GetType(), o, false);
        }

        public static TJSVariable ConvertFrom(object o, bool seeObjectAsCSharp)
        {
            if (o == null)
                return new TJSVariable(VarType.VOID);
            return ConvertFromType(o.GetType(), o, true);
        }

        string getPrintStr(string str)
        {
            string r = "";
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] < 32)
                {
                    r += "\\x" + ((int)str[i]).ToString("X");
                }
                else
                {
                    r += str[i];
                }
            }
            return "\"" + r + "\"";
        }

        public string ToFormatString(string format, bool multiLine = false, string indent = "", int recursiveLevel = 0)
        {
            switch (vt)
            {
                case VarType.VOID:
                    if (format != "V" && format != "v")
                        return "";
                    else
                        return "void";
                case VarType.NUMBER:
                    return num.ToString(format);
                case VarType.STRING:
                    if (format != "V" && format != "v")
                        return str;
                    else
                        return getPrintStr(str);
                case VarType.ARRAY:
                    {
                        var arr = ToArray();
                        if (arr.Count == 0)
                            return "[]";
                        if (recursiveLevel >= 10)
                            return "Array";
                        string res = "";
                        if (multiLine)
                        {
                            res += indent + "[";
                            for (int i = 0; i < arr.Count; i++)
                            {
                                if (i != 0)
                                    res += ",";
                                res += "\n" + indent + "  ";
                                res += arr[i].ToFormatString(format, multiLine, indent + "  ", recursiveLevel + 1);
                            }
                            res += "\n" + indent + "]";
                            return res;
                        }
                        else
                        {
                            res += "[";
                            for (int i = 0; i < arr.Count; i++)
                            {
                                if (i != 0)
                                    res += ",";
                                res += arr[i].ToFormatString(format, multiLine, indent, recursiveLevel + 1);
                            }
                            res += "]";
                            return res;
                        }
                    }
                case VarType.DICTIONARY:
                    {
                        var dic = ToDic();
                        if (dic.Count == 0)
                            return "%[]";
                        if (recursiveLevel >= 10)
                            return "Dictionary";
                        string res = "";
                        var k = dic.Keys.ToList();
                        if (multiLine)
                        {
                            res += indent + "[";
                            for (int i = 0; i < k.Count; i++)
                            {
                                if (i != 0)
                                    res += ",";
                                res += "\n" + indent + "  ";
                                res += getPrintStr(k[i]);
                                res += "=>";
                                res += dic[k[i]].ToFormatString(format, multiLine, indent + "  ", recursiveLevel + 1);
                            }
                            res += "\n" + indent + "]";
                            return res;
                        }
                        else
                        {
                            res += "[";
                            for (int i = 0; i < k.Count; i++)
                            {
                                if (i != 0)
                                    res += ",";
                                res += getPrintStr(k[i]);
                                res += "=>";
                                res += dic[k[i]].ToFormatString(format, multiLine, indent, recursiveLevel + 1);
                            }
                            res += "]";
                            return res;
                        }
                    }
                case VarType.FUNCTION:
                case VarType.PROPERTY:
                case VarType.CLASS:
                case VarType.CLOSURE:
                    if (format != "V" && format != "v")
                        return obj.ToString();
                    else
                        return "/*" + obj.ToString() + "*/";
                case VarType.UNDEFINED:
                    if (format != "V" && format != "v")
                        return "";
                    else
                        return "/*undefined*/";
                default:
                    return "";
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is TJSVariable)
                return StrictEqual((TJSVariable)obj);
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class TJSVariableFormatProvider : IFormatProvider, ICustomFormatter
    {
        int idx = 0;
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg.GetType() != typeof(TJSVariable))
                return arg.ToString();
            return ((TJSVariable)arg).ToFormatString(format);
        }

        public object GetFormat(Type formatType)
        {
            //UnityEngine.Debug.Log(formatType);
            return this;
        }
    }

    public class CustomComparer<T> : IComparer<T>
    {
        Comparison<T> comp;

        public CustomComparer(Comparison<T> c)
        {
            comp = c;
        }

        public int Compare(T x, T y)
        {
            return comp(x, y);
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

        Dictionary<string, TJSClass4CSharp> registerCSharpClass = new Dictionary<string, TJSClass4CSharp>();

#region BuiltinFunction
        public TJSVariable GetParam(int idx, TJSVariable[] param, int start, int num)
        {
            if (idx < 0)
                idx += num;
            if (idx < 0 || idx >= num || param[start + idx] == null)
                return new TJSVariable(VarType.VOID);
            return param[start + idx];
        }

        public TJSVariable GetParam(int idx, TJSVariable[] param, int start, int num, TJSVariable defaultValue)
        {
            if (idx < 0)
                idx += num;
            if (idx < 0 || idx >= num)
                return defaultValue;
            if (param[start + idx] == null)
                return new TJSVariable(VarType.VOID);
            return param[start + idx];
        }

        //some common built-in function
        TJSVariable _toString(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            return new TJSVariable(self.ToString());
        }

        TJSVariable _get_length(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            if (self.IsString())
                return new TJSVariable(self.ToString().Length);
            else if (self.IsArray())
                return new TJSVariable(self.ToArray().Count);
            else if (self.IsDic())
                return new TJSVariable(self.ToDic().Count);
            throw new TJSException("context error, only string, array and dictionary has length property");
        }

        TJSVariable _set_length(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            if (self.IsArray())
            {
                var arr = self.ToArray();
                int newlen = GetParam(0, param, start, num).ToInt();
                if (newlen < 0)
                    throw new TJSException("length cannot be negative");
                if (newlen == 0)
                    arr.Clear();
                else if (newlen < arr.Count)
                    arr.RemoveRange(newlen, arr.Count - newlen);
                else if (newlen > arr.Count)
                    arr.AddRange(Enumerable.Repeat(new TJSVariable(VarType.VOID), newlen - arr.Count));
                return new TJSVariable(VarType.VOID);
            }
            throw new TJSException("context error, only array'length can be set");
        }

        TJSVariable _StartsWith(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            if (self.IsString())
            {
                if (num == 0)
                {
                    throw new TJSException("need one parameter");
                }
                return new TJSVariable(self.ToString().StartsWith(GetParam(0, param, start, num).ToString()));
            }
            throw new TJSException("context error, only string has this method");
        }

        TJSVariable _IndexOf(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            if (self.IsString())
            {
                if (num == 0)
                {
                    throw new TJSException("need at least one parameter");
                }
                int startpos = 0;
                if (num > 1)
                    startpos = GetParam(1, param, start, num).ToInt();
                string substr = GetParam(0, param, start, num).ToString();
                string str = self.ToString();
                return new TJSVariable(str.IndexOf(substr, startpos));
            }
            throw new TJSException("context error, only string has this method");
        }

        TJSVariable _ToLowerCase(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            if (self.IsString())
            {
                return new TJSVariable(self.ToString().ToLower());
            }
            throw new TJSException("context error, only string has this method");
        }

        TJSVariable _ToUpperCase(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            if (self.IsString())
            {
                return new TJSVariable(self.ToString().ToUpper());
            }
            throw new TJSException("context error, only string has this method");
        }

        TJSVariable _Substring(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            if (self.IsString())
            {
                if (num == 0)
                {
                    throw new TJSException("need at least one parameter");
                }
                int startpos = GetParam(0, param, start, num).ToInt();
                if (num > 1)
                {
                    int len = GetParam(1, param, start, num).ToInt();
                    return new TJSVariable(self.ToString().Substring(startpos, len));
                }
                return new TJSVariable(self.ToString().Substring(startpos));
            }
            throw new TJSException("context error, only string has this method");
        }

        TJSVariable _Sprintf(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            if (self.IsString())
            {
                throw new TJSException("not implemented");
            }
            throw new TJSException("context error, only string has this method");
        }

        TJSVariable _Format(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            if (self.IsString())
            {
                string str = self.ToString();
                List<object> args = new List<object>();
                for (int i = 0; i < num; i++)
                {
                    args.Add(GetParam(i, param, start, num));
                }
                return new TJSVariable(string.Format(new TJSVariableFormatProvider(), str, args.ToArray()));
            }
            throw new TJSException("context error, only string has this method");
        }

        TJSVariable _Split(TJSVariable _, TJSVariable[] param, int start, int num)
        {
            var res = GetParam(1, param, start, num).ToString().Split(new string[] { GetParam(0, param, start, num).ToString() },
                GetParam(3, param, start, num).ToBoolean() ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
            return TJSVariable.ConvertFrom(res);
        }

        TJSVariable _Join(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            TJSArray arr = (TJSArray)self.obj;
            List<string> args = new List<string>();
            bool skipEmpty = GetParam(2, param, start, num).ToBoolean();
            for (int i = 0; i < arr.arr.Count; i++)
            {
                if (skipEmpty && arr.arr[i].IsVoid())
                    continue;
                args.Add(arr.arr[i].ToString());
            }
            return new TJSVariable(string.Join(param[start].ToString(), args.ToArray()));
        }

        TJSVariable _Reverse(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            List<TJSVariable> newlist = new List<TJSVariable>();
            List<TJSVariable> arr = ((TJSArray)self.obj).arr;
            for (int i = arr.Count - 1; i >= 0; i--)
            {
                newlist.Add(arr[i]);
            }
            ((TJSArray)self.obj).arr = newlist;
            return new TJSVariable(VarType.VOID);
        }

        TJSVariable _Sort(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            List<TJSVariable> arr = ((TJSArray)self.obj).arr;
            string mode = GetParam(0, param, start, num).ToString();
            bool stable = GetParam(2, param, start, num).ToBoolean();
            Comparison<TJSVariable> comparison = (a, b) => {
                if (a == b)
                    return 0;
                if (a > b)
                    return 1;
                else
                    return -1;
            };
            if (mode == "-")
            {
                comparison = (a, b) => {
                    if (a == b)
                        return 0;
                    if (a > b)
                        return -1;
                    else
                        return 1;
                };
            }
            if (mode == "0")
            {
                comparison = (a, b) => {
                    var aa = a.ToDouble();
                    var bb = b.ToDouble();
                    if (Math.Abs(aa - bb) < 1e-14)
                        return 0;
                    if (aa > bb)
                        return 1;
                    else
                        return -1;
                };
            }
            if (mode == "9")
            {
                comparison = (a, b) => {
                    var aa = a.ToDouble();
                    var bb = b.ToDouble();
                    if (Math.Abs(aa - bb) < 1e-14)
                        return 0;
                    if (aa > bb)
                        return -1;
                    else
                        return 1;
                };
            }
            if (mode == "a")
            {
                comparison = (a, b) => {
                    var aa = a.ToString();
                    var bb = b.ToString();
                    return string.Compare(aa, bb);
                };
            }
            if (mode == "z")
            {
                comparison = (a, b) => {
                    var aa = a.ToString();
                    var bb = b.ToString();
                    return -string.Compare(aa, bb);
                };
            }
            if (stable)
            {
                arr = arr.OrderBy(x => x, new CustomComparer<TJSVariable>(comparison)).ToList();
            }
            else
            {
                arr.Sort(comparison);
            }
            return new TJSVariable(VarType.VOID);
        }

        void AssignArray(TJSVariable self, TJSVariable p)
        {
            TJSArray arr = (TJSArray)self.obj;
            if (p.vt == VarType.VOID)
                arr.arr.Clear();
            else if (p.vt == VarType.ARRAY)
            {
                arr.arr = new List<TJSVariable>();
                arr.arr.AddRange(((TJSArray)p.obj).arr);
            }
            else if (p.vt == VarType.DICTIONARY)
            {
                arr.arr = new List<TJSVariable>();
                TJSDictionary dic = (TJSDictionary)p.obj;
                foreach (var kv in dic.dic)
                {
                    arr.arr.Add(new TJSVariable(kv.Key));
                    arr.arr.Add(kv.Value);
                }
            }
            else
            {
                arr.arr = new List<TJSVariable>();
                arr.arr.Add(p);
            }

        }

        TJSVariable _Assign(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            TJSVariable p = GetParam(0, param, start, num);
            AssignArray(self, p);
            return new TJSVariable(VarType.VOID);
        }

        TJSVariable _AssignStruct(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            TJSVariable p = GetParam(0, param, start, num);
            AssignArray(self, p.DeepClone());
            return new TJSVariable(VarType.VOID);
        }

        TJSVariable _Clear(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            ((TJSArray)self.obj).arr.Clear();
            return new TJSVariable(VarType.VOID);
        }

        TJSVariable _Erase(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            int idx = GetParam(0, param, start, num).ToInt();
            ((TJSArray)self.obj).arr.RemoveAt(idx);
            return new TJSVariable(VarType.VOID);
        }

        TJSVariable _Remove(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            var v = GetParam(0, param, start, num);
            bool all = GetParam(1, param, start, num, new TJSVariable(true)).ToBoolean();
            if (!all)
                ((TJSArray)self.obj).arr.Remove(v);
            else
                ((TJSArray)self.obj).arr.RemoveAll((x) => x.StrictEqual(v));
            return new TJSVariable(VarType.VOID);
        }

        TJSVariable _Insert(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            int idx = GetParam(0, param, start, num).ToInt();
            var arr = ((TJSArray)self.obj).arr;
            if (idx < 0)
                idx += arr.Count;
            if (idx < 0 || idx >= arr.Count)
                throw new TJSException("下标溢出");
            arr.Insert(idx, GetParam(1, param, start, num));
            return new TJSVariable(VarType.VOID);
        }

        TJSVariable _Add(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            var p = GetParam(0, param, start, num);
            var arr = ((TJSArray)self.obj).arr;
            arr.Add(p);
            return new TJSVariable(arr.Count);
        }

        TJSVariable _Find(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            var p = GetParam(0, param, start, num);
            int idx = GetParam(1, param, start, num).ToInt();
            var arr = ((TJSArray)self.obj).arr;
            return new TJSVariable(arr.FindIndex(idx, (x) => x.StrictEqual(p)));
        }
#endregion BuiltinFunction

        void initNumMethod()
        {
            TJSVariable self = new TJSVariable(numClass);
            numClass.SetField("toString", new TJSVariable(new TJSFunction(_toString, self)));
        }

        void initStringMethod()
        {
            TJSVariable self = new TJSVariable(strClass);
            strClass.SetField("length", new TJSVariable(new TJSProperty(_get_length, null, self)));
            strClass.SetField("toString", new TJSVariable(new TJSFunction(_toString, self)));
            strClass.SetField("startsWith", new TJSVariable(new TJSFunction(_StartsWith, self)));
            strClass.SetField("indexOf", new TJSVariable(new TJSFunction(_IndexOf, self)));
            strClass.SetField("toLowerCase", new TJSVariable(new TJSFunction(_ToLowerCase, self)));
            strClass.SetField("toUpperCase", new TJSVariable(new TJSFunction(_ToUpperCase, self)));
            {
                var func = new TJSFunction(_Substring, self);
                strClass.SetField("substring", new TJSVariable(func));
                strClass.SetField("substr", new TJSVariable(func));
            }
            strClass.SetField("format", new TJSVariable(new TJSFunction(_Format, self)));
        }

        void initArrayMethod()
        {
            var self = new TJSVariable(arrClass);
            {
                var prop = new TJSProperty(_get_length, _set_length, self);
                arrClass.SetField("length", new TJSVariable(prop));
                arrClass.SetField("count", new TJSVariable(prop));
            }
            arrClass.SetField("split", new TJSVariable(new TJSFunction(_Split, self)));
            arrClass.SetField("join", new TJSVariable(new TJSFunction(_Join, self)));
            arrClass.SetField("reverse", new TJSVariable(new TJSFunction(_Reverse, self)));
            arrClass.SetField("sort", new TJSVariable(new TJSFunction(_Sort, self)));
            arrClass.SetField("assign", new TJSVariable(new TJSFunction(_Assign, self)));
            arrClass.SetField("copy", new TJSVariable(new TJSFunction(_Assign, self)));
            arrClass.SetField("assignStruct", new TJSVariable(new TJSFunction(_AssignStruct, self)));
            arrClass.SetField("deepcopy", new TJSVariable(new TJSFunction(_AssignStruct, self)));
            arrClass.SetField("clear", new TJSVariable(new TJSFunction(_Clear, self)));
            arrClass.SetField("erase", new TJSVariable(new TJSFunction(_Erase, self)));
            arrClass.SetField("remove", new TJSVariable(new TJSFunction(_Remove, self)));
            arrClass.SetField("insert", new TJSVariable(new TJSFunction(_Insert, self)));
            arrClass.SetField("add", new TJSVariable(new TJSFunction(_Add, self)));
            arrClass.SetField("find", new TJSVariable(new TJSFunction(_Find, self)));
        }

        private TJSVM()
        {
            //make these class be extendable
            _global.SetField("int", new TJSVariable(numClass));
            _global.SetField("string", new TJSVariable(strClass));
            _global.SetField("array", new TJSVariable(arrClass));
            _global.SetField("dictionary", new TJSVariable(dicClass));

            initNumMethod();
            initStringMethod();
            initArrayMethod();

            _global.SetField("log", new TJSVariable(new TJSFunction(Log, new TJSVariable(VarType.VOID))));
        }

        private static TJSVariable Log(TJSVariable _this, TJSVariable[] param, int start, int length)
        {
            string str = "";
            for (int i = 0; i < length; i++)
            {
                if (i != 0)
                    str += ',';
                str += param[i + start].ToFormatString("");
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
                            if (stack[c.op1 + neg].vt == VarType.PROPERTY)
                            {
                                ((TJSProperty)stack[c.op1 + neg].obj).SetValue(stack[c.op2 + neg]);
                            }
                            else
                            {
                                stack[c.op1 + neg] = stack[c.op2 + neg];
                            }
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
                                if (stack[c.op1 + neg].vt != VarType.FUNCTION && stack[c.op1 + neg].vt != VarType.CLASS)
                                {
                                    throw new TJSException("the object be called must be a function");
                                }
                                stack[c.op1 + neg] = stack[c.op1 + neg].obj.CallAsFunc(stack, c.op2 + neg, c.op3 - c.op2);
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
                                if (c.op3 > c.op2)
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
                            {
                                var cla = stack[c.op1 + neg];
                                if (!cla.IsClass())
                                {
                                    if (c.op3 != -1)
                                    {
                                        cla = VM._global.getMember(code.consts[c.op3].ToString());
                                    }
                                    else if (cla.IsString())
                                    {
                                        cla = VM._global.getMember(cla.ToString());
                                    }
                                    if (!cla.IsClass())
                                    {
                                        throw new TJSException("new后应接一个类，或者是值为类的表达式");
                                    }
                                    stack[c.op1 + neg] = cla;
                                }
                            }
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
                catch (Exception e)
                {
                    TJSException ee = new TJSException(e.Message);
                    if (start < code.codes.Count)
                        ee.AddTrace(code.codes[start].line, code.codes[start].offset);
                    throw ee;
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

        public void AddCSharpClass(Type type, bool hasInstance, string varname)
        {
            var c = new TJSClass4CSharp(type, hasInstance);
            registerCSharpClass.Add(varname, c);
            _global.SetField(varname, new TJSVariable(c));
        }

        public void AddCSharpObjectAsStatic(object o, string varname)
        {
            var c = new TJSClass4CSharp(o);
            registerCSharpClass.Add(varname, c);
            _global.SetField(varname, new TJSVariable(c));
        }

        public TJSVariable WrapCSharpObject(object o)
        {
            string varname = o.GetType().Name;
            if (!registerCSharpClass.ContainsKey(varname))
            {
                AddCSharpClass(o.GetType(), true, varname);
            }
            var defclass = registerCSharpClass[varname];
            return new TJSVariable(new TJSClass4CSharp(defclass, o.GetType(), o));
        }
    }

    public class TJSCSharpFunction : TJSFunction
    {
        internal List<MethodInfo> m;

        TJSClass4CSharp csthis;

        public TJSCSharpFunction(List<MethodInfo> m, TJSClass4CSharp _this) : base()
        {
            if (m != null)
                this.m = m;
            else
                this.m = new List<MethodInfo>();
            csthis = _this;
        }

        public override TJSVariable CallAsFunc(TJSVariable[] param, int start, int length)
        {
            return csthis.CallMethod(m, param, start, length);
        }
    }

    public class TJSClass4CSharp : TJSClass
    {
        internal Type tp;

        internal object obj = null;

        TJSFunction create = null;

        Dictionary<string, List<MethodInfo>> methodsInfo = new Dictionary<string, List<MethodInfo>>();

        Dictionary<string, FieldInfo> fieldsInfo = new Dictionary<string, FieldInfo>();

        Dictionary<string, PropertyInfo> propertiesInfo = new Dictionary<string, PropertyInfo>();

        List<PropertyInfo> indexer = new List<PropertyInfo>();

        internal TJSClass4CSharp(Type classType, bool hasInstance) : base()
        {
            tp = classType;
            canInstantiate = hasInstance;
            if (canInstantiate)
            {
                makeCreateFunction();
            }
            InitReflectionTable();
        }

        internal TJSClass4CSharp(object o) : base()
        {
            tp = o.GetType();
            obj = o;
            canInstantiate = false;
            InitReflectionTable();
        }

        protected void InitReflectionTable()
        {
            var methods = tp.GetMethods(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Instance);
            foreach(var m in methods)
            {
                if(!methodsInfo.ContainsKey(m.Name))
                {
                    methodsInfo[m.Name] = new List<MethodInfo>();
                }
                methodsInfo[m.Name].Add(m);
            }
            var props = tp.GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Instance);
            foreach (var p in props)
            {
                if (p.GetIndexParameters().Length == 0)
                    propertiesInfo[p.Name] = p;
                else if (p.GetIndexParameters().Length == 1)
                    indexer.Add(p);
            }
            var fields = tp.GetFields(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Instance);
            foreach(var f in fields)
            {
                fieldsInfo[f.Name] = f;
            }
        }

        internal TJSClass4CSharp(TJSClass4CSharp defclass, Type classType, object o) : base(defclass)
        {
            tp = classType;
            obj = o;
            create = defclass.create;
        }

        public override TJSVariable CreateInstance(TJSVariable[] param, int start, int length)
        {
            if (!canInstantiate)
            {
                throw new TJSException("该类<" + name + ">不能实例化");
            }
            var res = create.CallAsFunc(param, start, length);
            return res;
        }

        internal static MethodInfo FindMatchMethod(MethodInfo[] methods, TJSVariable[] param, int start, int length, out List<object> args)
        {
            MethodInfo select = null;
            args = new List<object>();
            foreach (var m in methods)
            {
                args.Clear();
                bool suc = true;
                var p = m.GetParameters();
                if (p.Length == length)
                {
                    for (int i = 0; i < length; i++)
                    {
                        args.Add(param[start + i].TryConvertToTypeStrict(p[i].ParameterType, out suc));
                        if(!suc)
                        {
                            break;
                        }
                    }
                    if(suc)
                    {
                        select = m;
                        break;
                    }
                    continue;
                }
            }
            return select;
        }

        public bool makeCreateFunction()
        {
            var methods = tp.GetMethods(BindingFlags.Public | BindingFlags.CreateInstance);
            if (methods.Length == 0)
            {
                canInstantiate = false;
                return false;
            }
            else
            {
                create = new TJSFunction((TJSVariable _this, TJSVariable[] param, int start, int length) =>
                {
                    MethodInfo select = FindMatchMethod(methods, param, start, length, out List<object> args);
                    if (select == null)
                    {
                        throw new TJSException("找不到相匹配的构造函数");
                    }
                    var ret = select.Invoke(null, args.ToArray());
                    return new TJSVariable(new TJSClass4CSharp(this, tp, ret));
                }, new TJSVariable(this));
            }
            return true;
        }

        public TJSVariable CallMethod(List<MethodInfo> ms, TJSVariable[] param, int start, int length, object _this)
        {
            MethodInfo m = FindMatchMethod(ms.ToArray(), param, start, length, out List<object> args);
            var r = m.ReturnType;
            if (r == null || r == typeof(void))
            {
                m.Invoke(_this, args.ToArray());
                return new TJSVariable(VarType.VOID);
            }
            var ret = m.Invoke(_this, args.ToArray());
            return TJSVariable.ConvertFromType(r, ret, true);
        }

        internal TJSVariable CallMethod(List<MethodInfo> m, TJSVariable[] param, int start, int length)
        {
            return CallMethod(m, param, start, length, obj);
        }


        public TJSVariable GetFieldValue(FieldInfo f, object _this)
        {
            return TJSVariable.ConvertFrom(f.GetValue(_this), true);
        }

        public void SetFieldValue(FieldInfo f, TJSVariable v, object _this)
        {
            f.SetValue(_this, v.ConvertToType(f.FieldType));
        }

        public TJSVariable GetPropertyValue(PropertyInfo p, object _this)
        {
            return TJSVariable.ConvertFrom(p.GetValue(_this), true);
        }

        public void SetPropertyValue(PropertyInfo p, TJSVariable v, object _this)
        {
            p.SetValue(_this, v.ConvertToType(p.PropertyType));
        }

        public TJSVariable GetIndexerValue(TJSVariable idx, object _this)
        {
            if (indexer.Count == 0)
            {
                return new TJSVariable(VarType.UNDEFINED);
            }
            else if (indexer.Count == 1)
            {
                var res = indexer[0].GetValue(_this, new object[] { idx.ConvertToType(indexer[0].GetIndexParameters()[0].ParameterType) });
                return TJSVariable.ConvertFrom(res);
            }
            else
            {
                foreach(var index in indexer)
                {
                    var pt = index.GetIndexParameters()[0].ParameterType;
                    bool suc;
                    object v = idx.TryConvertToTypeStrict(pt, out suc);
                    if(suc)
                    {
                        var res = index.GetValue(_this, new object[] { idx.ConvertToType(pt) });
                        return TJSVariable.ConvertFrom(res);
                    }
                }
                return new TJSVariable(VarType.UNDEFINED);
            }
        }

        public bool SetIndexerValue(TJSVariable idx, TJSVariable value, object _this)
        {
            if (indexer.Count == 0)
            {
                return false;
            }
            else if (indexer.Count == 1)
            {
                bool suc;
                object o = idx.TryConvertToType(indexer[0].GetIndexParameters()[0].ParameterType, out suc);
                if (!suc)
                    return false;
                indexer[0].SetValue(_this, o);
                return true;
            }
            else
            {
                foreach (var index in indexer)
                {
                    var pt = index.GetIndexParameters()[0].ParameterType;
                    bool suc;
                    object v = idx.TryConvertToTypeStrict(pt, out suc);
                    if (suc)
                    {
                        index.SetValue(_this, idx.ConvertToType(pt));
                        return true;
                    }
                }
                return false;
            }
        }

        // 暂不允许TJSClass4CSharp和脚本里的Class成员混用
        public virtual TJSVariable GetFieldForChild(int name, TJSClass4CSharp _this)
        {
            return GetIndexerValue(new TJSVariable(name), _this.obj);
        }

        public override TJSVariable GetField(int name)
        {
            if (defineClass == null)
                return GetFieldForChild(name, this);
            else
                return ((TJSClass4CSharp)defineClass).GetFieldForChild(name, this);
        }

        public virtual TJSVariable GetFieldForChild(string name, TJSClass4CSharp _this)
        {
            if(methodsInfo.ContainsKey(name))
            {
                return new TJSVariable(new TJSCSharpFunction(methodsInfo[name], _this));
            }
            if (propertiesInfo.ContainsKey(name))
            {
                return GetPropertyValue(propertiesInfo[name], _this.obj);
            }
            if (fieldsInfo.ContainsKey(name))
            {
                return GetFieldValue(fieldsInfo[name], _this.obj);
            }
            return GetIndexerValue(new TJSVariable(name), _this.obj);
        }

        public override TJSVariable GetField(string name)
        {
            if (defineClass == null)
            {
                if (methodsInfo.ContainsKey(name))
                {
                    return new TJSVariable(new TJSCSharpFunction(methodsInfo[name], this));
                }
                if (propertiesInfo.ContainsKey(name))
                {
                    return GetPropertyValue(propertiesInfo[name], obj);
                }
                if (fieldsInfo.ContainsKey(name))
                {
                    return GetFieldValue(fieldsInfo[name], obj);
                }
                return GetIndexerValue(new TJSVariable(name), obj);
            }
            else
            {
                var c = (TJSClass4CSharp)defineClass;
                return c.GetFieldForChild(name, this);
            }
        }

        public virtual void SetFieldForChild(int name, TJSVariable value, TJSClass4CSharp _this)
        {
            SetIndexerValue(new TJSVariable(name), value, _this.obj);
        }

        public override void SetField(int name, TJSVariable value)
        {
            SetIndexerValue(new TJSVariable(name), value, this);
        }

        public virtual void SetFieldForChild(string name, TJSVariable value, TJSClass4CSharp _this)
        {
            if (propertiesInfo.ContainsKey(name))
            {
                SetPropertyValue(propertiesInfo[name], value, _this.obj);
            }
            else if (fieldsInfo.ContainsKey(name))
            {
                SetFieldValue(fieldsInfo[name], value, _this.obj);
            }
            else
            {
                SetIndexerValue(new TJSVariable(name), value, _this.obj);
            }
        }

        public override void SetField(string name, TJSVariable value)
        {
            if (defineClass == null)
            {
                if (propertiesInfo.ContainsKey(name))
                {
                    SetPropertyValue(propertiesInfo[name], value, obj);
                }
                else if (fieldsInfo.ContainsKey(name))
                {
                    SetFieldValue(fieldsInfo[name], value, obj);
                }
                else
                {
                    SetIndexerValue(new TJSVariable(name), value, obj);
                }
            }
            else
            {
                var c = (TJSClass4CSharp)defineClass;
                c.SetFieldForChild(name, value, this);
            }
        }

    }
}