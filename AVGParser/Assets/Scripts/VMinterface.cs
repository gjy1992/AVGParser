using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVGParser
{
    public interface VMVariable
    {
        string ToString();

        int ToInt();

        double ToDouble();

        bool ToBoolean();

        Dictionary<string, VMVariable> ToDic();

        List<VMVariable> ToArray();

        VMVariable RunAsFunc();

        bool IsVoid();

        bool IsInt();

        bool IsDouble();

        bool IsString();

        bool IsBoolean();

        bool IsDic();

        bool IsArray();

        bool IsClass();
    }

    public interface VMinterface
    {
        VMVariable Eval(string exp);

        //VMVariable EvalInClosure(string exp, VMVariable closure);
    }
}
