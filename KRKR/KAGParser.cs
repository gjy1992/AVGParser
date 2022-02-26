using System.Collections;
using System.Collections.Generic;
using System;
using System.Collections.Specialized;

namespace AVGParser.KRKR
{
    public class Command
    {
        public string CommandName { get; internal set; } = "";
        public Dictionary<string, string> CommandParams { get; internal set; } = new Dictionary<string, string>();
        public string ScriptName { get; internal set; } = "";
        public string LabelName { get; internal set; } = null;
        public int LineNumber { get; internal set; }
    }

    public class MacroInfo
    {
        public string MacroName { get; internal set; } = "";
        public string MacroLine { get; internal set; } = "";
    }

    public class MacroBuffer
    {
        public string MacroString { get; internal set; } = "";
        public int Offset { get; internal set; }
        public int PrevOffset { get; internal set; }
        public Dictionary<string, string> MacroParams { get; internal set; } = new Dictionary<string, string>();
        public Dictionary<string, TJSVariable> cacheTJSDic { get; internal set; } = null;
    }

    public class CallStackInfo
    {
        public string ScriptName { get; internal set; } = "";
        public string LabelName { get; internal set; } = null;
        public int LineNumber { get; internal set; }
        public int LineOffset { get; internal set; }
        public int PrevLineNumber { get; internal set; }
        public int PrevLineOffset { get; internal set; }
        public List<MacroBuffer> buffer { get; internal set; } = new List<MacroBuffer>();
    }

    public enum IfState
    {
        If,
        ElseIf,
        Else,
    }

    [Serializable]
    public class SaveState
    {
        public int TrueIfDepth { get; internal set; } = 0;      //count of @if blocks which current branch is active
        public int PassedIfDepth { get; internal set; } = 0;    //count of @if blocks which one branch of it has been entered
        public int AllIfDepth { get; internal set; } = 0;   //count of @if blocks
        public Stack<IfState> IfState { get; internal set; } = new Stack<IfState>();
        public List<CallStackInfo> CallStack { get; internal set; } = new List<CallStackInfo>();

        internal SaveState clone()
        {
            return MemberwiseClone() as SaveState;
        }
    }

    // --------------------------------------------------------------------------------
    /// <summary>
    /// KAGParser class, parse kag-type script
    /// </summary>
    /// <remarks>
    /// only support line comment by ; //
    /// ; must be line beginning
    /// no texttag support
    /// support multi-times return by @return times=2
    /// macro and endmacro must occupy one single line and begin at line start
    /// macro names are case sensitive
    /// </remarks>
    // --------------------------------------------------------------------------------
    public class KAGParser
    {
        /// <summary>whether process SpecialTags internal, default is true</summary>
        /// <remarks>
        /// If set true, special tags like "jump" will be handled internal
        /// If set false, will return a Command class as normal command, offen used for scripts analysis
        /// All special tags are:
        /// if endif else elseif emb macro endmacro erasemacro jump call return
        /// there is small difference with raw krkr special tags
        /// </remarks>
        public bool ProcessSpecialTags = true;

        /// <summary>prohibit auto savepoint at savable label, such as in menu script</summary>
        public bool NoSaveGlobal = false;

        /// <summary>
        /// whether parse all continuous texts as one command, default is false (same as kag)
        /// </summary>
        public bool ParseTextTogether = false;

        /// <summary>
        /// VM used for eval exp
        /// </summary>
        public TJSVM VM = TJSVM.VM;

        public string ErrorMessage { get; internal set; }

        public string CurrentScript
        {
            get
            {
                int cc = currentSave.CallStack.Count;
                if (cc <= 0)
                    return null;
                return currentSave.CallStack[cc - 1].ScriptName;
            }
        }

        public string CurrentLabel
        {
            get
            {
                int cc = currentSave.CallStack.Count;
                if (cc <= 0)
                    return null;
                return currentSave.CallStack[cc - 1].LabelName;
            }
        }

        public int CurrentLineNumber
        {
            get
            {
                int cc = currentSave.CallStack.Count;
                if (cc <= 0)
                    return -1;
                return currentSave.CallStack[cc - 1].LineNumber;
            }
        }

        public int PrevLineNumber
        {
            get
            {
                int cc = currentSave.CallStack.Count;
                if (cc <= 0)
                    return -1;
                return currentSave.CallStack[cc - 1].PrevLineNumber;
            }
        }

        public int CurrentLineOffset
        {
            get
            {
                int cc = currentSave.CallStack.Count;
                if (cc <= 0)
                    return -1;
                return currentSave.CallStack[cc - 1].LineOffset;
            }
        }

        public int PrevLineOffset
        {
            get
            {
                int cc = currentSave.CallStack.Count;
                if (cc <= 0)
                    return -1;
                return currentSave.CallStack[cc - 1].PrevLineOffset;
            }
        }

        //callbacks
        public Func<string, string> LoadScriptCallback;

        //scriptName, label, isSaveLabel
        public Action<string, string, bool> OnLabel;

        public SaveState LastSavepointState
        {
            get
            {
                return lastSave.clone();
            }
        }

        public Dictionary<string, MacroInfo> macros { get; internal set; } = new Dictionary<string, MacroInfo>();

        class ScriptCache
        {
            public string[] lines = null;
            public Dictionary<string, int> labelmap = new Dictionary<string, int>();
            public Dictionary<string, bool> labelsaveinfo = new Dictionary<string, bool>();
            public bool parseOver = false;
            public int parseLineNumber = 0;
        }

        Dictionary<string, ScriptCache> scriptCache = new Dictionary<string, ScriptCache>();

        SaveState lastSave = null;
        SaveState currentSave = new SaveState();

        ScriptCache currentScriptCache = null;

        //current macro when parse macro command
        MacroInfo currentMacro = null;
        bool currentMacroValid = false;

        CallStackInfo currentCall
        {
            get
            {
                int cc = currentSave.CallStack.Count;
                if (cc <= 0)
                    return null;
                return currentSave.CallStack[cc - 1];
            }
        }

        //current macro context
        MacroBuffer currentRunMacro
        {
            get
            {
                var macrobuf = currentCall?.buffer;
                if (macrobuf == null || macrobuf.Count == 0)
                    return null;
                return macrobuf[macrobuf.Count - 1];
            }
        }

        bool currentIf
        {
            get
            {
                return currentSave.TrueIfDepth == currentSave.AllIfDepth;
            }
        }

        bool isLineHead = true;

        public TJSVariable MP(TJSVariable self, TJSVariable[] param, int start, int num)
        {
            var macrobuf = currentRunMacro;
            if (macrobuf.cacheTJSDic == null)
            {
                var dic = macrobuf.MacroParams;
                var tjsdic = new Dictionary<string, TJSVariable>();
                foreach (var it in dic)
                {
                    tjsdic[it.Key] = new TJSVariable(it.Value);
                }
                macrobuf.cacheTJSDic = tjsdic;
            }
            return new TJSVariable(new TJSDictionary(macrobuf.cacheTJSDic));
        }

        public KAGParser()
        {
            //register mp property
            VM._global.RemoveField("mp");
            VM._global.SetField("mp", new TJSVariable(new TJSProperty(MP, null, new TJSVariable())));
        }

        /// <summary>
        /// recover KAGParser state from savedata
        /// </summary>
        /// <param name="state"></param>
        public void LoadState(SaveState state)
        {
            lastSave = state.clone();
            currentSave = state.clone();
        }

        /// <summary>
        /// Manual set the content of a script
        /// </summary>
        /// <param name="scriptName"></param>
        /// <param name="content"></param>
        public void SetScriptContent(string scriptName, string content)
        {
            scriptCache[scriptName] = new ScriptCache();
            content = content.Replace("\r\n", "\n");
            content = content.Replace("\r", "\n");
            scriptCache[scriptName].lines = content.Split('\n');
        }

        /// <summary>
        /// Load a new script and start from a label
        /// </summary>
        public void LoadScript(string scriptName, string label = null)
        {
            if (currentSave.CallStack.Count == 0)
            {
                newCallInfo();
            }
            if (scriptName == null)
                scriptName = CurrentScript;
            if (!scriptCache.ContainsKey(scriptName))
            {
                if (LoadScriptCallback != null)
                {
                    //should handle error inside the function
                    var text = LoadScriptCallback(scriptName);
                    if (text == null)
                        return;
                    scriptCache[scriptName] = new ScriptCache();
                    text = text.Replace("\r\n", "\n");
                    text = text.Replace("\r", "\n");
                    scriptCache[scriptName].lines = text.Split('\n');
                }
                else
                {
                    var asset = UnityEngine.Resources.Load<UnityEngine.TextAsset>(scriptName);
                    if (asset == null)
                    {
                        UnityEngine.Debug.LogError($"cannot find script <{scriptName}>");
                        return;
                    }
                    else
                    {
                        scriptCache[scriptName] = new ScriptCache();
                        var text = asset.text;
                        text = text.Replace("\r\n", "\n");
                        text = text.Replace("\r", "\n");
                        scriptCache[scriptName].lines = text.Split('\n');
                    }
                }
            }
            currentScriptCache = scriptCache[scriptName];
            if (scriptName != CurrentScript)
            {
                //clear if info
                currentSave.TrueIfDepth = currentSave.AllIfDepth = 0;
            }
            currentCall.ScriptName = scriptName;
            if (label != null && label.StartsWith("*"))
                label = label.Substring(1);
            currentCall.LabelName = label;
            int line = searchLabel(label);
            if (line == -1)
            {
                UnityEngine.Debug.LogError($"label <{label}> not exist in file <{scriptName}>");
                currentCall.LineNumber = 0;
            }
            else
            {
                currentCall.LineNumber = line;
            }
            currentCall.LineOffset = 0;
            currentCall.buffer.Clear();
            if (lastSave == null)
                lastSave = currentSave.clone();
        }

        /// <summary>
        /// Call into a new label in script, if script is null, call label in current script
        /// </summary>
        public void Call(string scriptName = null, string label = null)
        {
            newCallInfo();
            LoadScript(scriptName, label);
        }

        /// <summary>
        /// Call label in current script, same as Call(null, label)
        /// </summary>
        public void CallLabel(string label)
        {
            Call(null, label);
        }

        /// <summary>
        /// Jump to a label, same as LoadScript(scriptName, label)
        /// </summary>
        public void Jump(string scriptName, string label = null)
        {
            LoadScript(scriptName, label);
        }

        /// <summary>
        /// Return several times, default is 1
        /// </summary>
        public void Return(int times = 1)
        {
            while (times > 0)
            {
                if (currentSave.CallStack.Count <= 1)
                {
                    UnityEngine.Debug.LogError("cannot return from topest level");
                    return;
                }
                currentSave.CallStack.RemoveAt(currentSave.CallStack.Count - 1);
                times--;
            }
        }


        /// <summary>
        /// trigger a savepoint, save state internally
        /// </summary>
        public void Savepoint()
        {
            lastSave = currentSave.clone();
        }

        /// <summary>
        /// Get next command, the text is converted to text command, emb is also parsed and return a text command.
        /// Whether special tag is processed internal is depended on the <c>ProcessSpecialTags</c> variable
        /// </summary>
        /// <returns></returns>
        public Command GetNextTag()
        {
            while (true)
            {
                var text = getCurLine();
                while (text != null && text.Length == 0)
                {
                    proceedOffset(0);
                    text = getCurLine();
                }
                if (text == null)
                    return null;
                int n = 0;
                while (n < text.Length && (text[n] == '\n' || text[n] == '\t'))
                    n++;
                if (n > 0)
                {
                    proceedOffset(n);
                    text = text.Substring(n);
                }
                if (text[0] == '*' && isLineHead)
                {
                    //label
                    string label = text.Substring(1);
                    proceedOffset(text.Length);
                    if (currentMacro != null)
                    {
                        error("label cannot lies inside a macro");
                        continue;
                    }
                    if (currentIf)
                    {
                        bool isSave = false;
                        if (label.EndsWith("|"))
                        {
                            isSave = true;
                            label = label.Substring(0, label.Length - 1);
                        }
                        OnLabel?.Invoke(CurrentScript, label, isSave);
                        if (isSave && !NoSaveGlobal)
                        {
                            lastSave = currentSave.clone();
                        }
                    }
                    continue;
                }
                else if ((text[0] == ';' && isLineHead) || text.StartsWith("//"))
                {
                    //line comment
                    proceedOffset(text.Length);
                    continue;
                }
                else if ((text[0] == '@' && isLineHead) || text[0] == '[')
                {
                    //command
                    bool isAt = text[0] == '@';
                    int j = 1;
                    Command cmd = new Command();
                    cmd.ScriptName = CurrentScript;
                    cmd.LineNumber = CurrentLineNumber;
                    cmd.LabelName = CurrentLabel;
                    while (j < text.Length && (char.IsLetterOrDigit(text[j]) || text[j] == '_' || text[j] > 127))
                    {
                        if (j == 1 && char.IsDigit(text[j]))
                            break;
                        j++;
                    }
                    cmd.CommandName = text.Substring(1, j - 1);
                    if (cmd.CommandName == "")
                    {
                        error("illegal command name");
                        if (isAt)
                            proceedOffset(text.Length);
                        else
                            proceedOffset(skipToChar(text, "]") + 1);
                        return null;
                    }
                    if (isAt)
                        j = skipToChar(text, " \t", j);
                    else
                        j = skipToChar(text, " \t]", j);
                    while (true)
                    {
                        while (j < text.Length && char.IsWhiteSpace(text[j]))
                            j++;
                        if (!isAt)
                        {
                            if (j < text.Length && text[j] == ']')
                            {
                                j++;
                                break;
                            }
                        }
                        if (j >= text.Length)
                        {
                            if (isAt)
                            {
                                break;
                            }
                            else
                            {
                                //read next line
                                proceedOffset(j);
                                text = getCurLine();
                                j = 0;
                            }
                        }
                        int k = j;
                        while (k < text.Length && (char.IsLetterOrDigit(text[k]) || text[j] == '_' || text[k] > 127))
                        {
                            if (k == j && char.IsDigit(text[k]))
                                break;
                            k++;
                        }
                        var propname = text.Substring(j, k - j);
                        if (propname == "")
                        {
                            error("illegal property name");
                            if (isAt)
                                proceedOffset(text.Length);
                            else
                                proceedOffset(skipToChar(text, "]") + 1);
                            return null;
                        }
                        var value = "true"; //this is default value
                        if (text[k] == '=')
                        {
                            //parse value
                            k++;
                            if (isAt)
                                j = skipToChar(text, " \t", k);
                            else
                                j = skipToChar(text, " \t]", k);
                            value = text.Substring(k, j - k);
                            if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length > 1)
                            {
                                value = value.Substring(1, value.Length - 2);
                            }
                            else if (value.StartsWith("&"))
                            {
                                value = value.Substring(1);
                                if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length > 1)
                                {
                                    value = value.Substring(1, value.Length - 2);
                                }
                                if (currentIf)
                                {
                                    try
                                    {
                                        TJSVariable res = VM.Eval(value);
                                        value = res.ToString();
                                    }
                                    catch (Exception e)
                                    {
                                        error(e.Message);
                                        return null;
                                    }
                                }
                            }
                            else if (value.StartsWith("%"))
                            {
                                if (currentRunMacro == null)
                                {
                                    error("cannot use %variable in non-macro context");
                                    return null;
                                }
                                var valuelist = value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                var dic = currentRunMacro?.MacroParams;
                                value = "";
                                foreach (var item in valuelist)
                                {
                                    if (item.StartsWith("%"))
                                    {
                                        if (dic.ContainsKey(item.Substring(1)))
                                        {
                                            value = item.Substring(1);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        value = item;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (isAt)
                                j = skipToChar(text, " \t", k);
                            else
                                j = skipToChar(text, " \t]", k);
                        }
                        cmd.CommandParams[propname] = value;
                    }
                    proceedOffset(j);
                    if (ProcessSpecialTags)
                    {
                        //handle jump, call, iscript etc
                        bool isSpecial = processSpecialTags(cmd);
                        if (currentMacro != null && cmd.CommandName != "macro")
                        {
                            if (isLineHead && currentMacro.MacroLine.Length > 0)
                                currentMacro.MacroLine += "\n";
                            currentMacro.MacroLine += text.Substring(0, j);
                            continue;
                        }
                        // if currentIf==false, then all command is see as special
                        else if (isSpecial)
                        {
                            continue;
                        }
                    }
                    return cmd;
                }
                else
                {
                    //text
                    if (currentIf && currentMacro == null)
                    {
                        Command cmd = new Command();
                        cmd.ScriptName = CurrentScript;
                        cmd.LineNumber = CurrentLineNumber;
                        cmd.LabelName = CurrentLabel;
                        cmd.CommandName = "text";
                        if (ParseTextTogether && text.Length > 1)
                        {
                            int end = 0;
                            int ch = text[end];
                            int ch2 = text[end + 1];
                            while (ch != '[' && !(ch == '/' && ch2 == '/'))
                            {
                                end++;
                                if (end >= text.Length)
                                    break;
                                ch = ch2;
                                if (end + 1 < text.Length)
                                    ch2 = text[end + 1];
                                else
                                    ch2 = 0;
                            }
                            cmd.CommandParams["text"] = text.Substring(0, end);
                            proceedOffset(end);
                        }
                        else
                        {
                            cmd.CommandParams["text"] = text.Substring(0, 1);
                            proceedOffset(1);
                        }
                        return cmd;
                    }
                    else
                    {
                        if (currentMacro != null)
                        {
                            if (isLineHead && currentMacro.MacroLine.Length > 0)
                                currentMacro.MacroLine += "\n";
                            currentMacro.MacroLine += text[0];
                        }
                        proceedOffset(1);
                        continue;
                    }
                }
            }
        }

        //handle jump, call, iscript etc
        bool processSpecialTags(Command cmd)
        {
            if (currentMacro != null)
            {
                if (cmd.CommandName == "endmacro")
                {
                    if (currentMacroValid)
                    {
                        //manual pop macro stack
                        currentMacro.MacroLine += "\n[macropop]";
                        macros[currentMacro.MacroName] = currentMacro;
                    }
                    currentMacro = null;
                }
                return true;
            }
            if (!currentIf)
            {
                if (cmd.CommandName == "if")
                {
                    currentSave.AllIfDepth++;
                }
                else if (cmd.CommandName == "else")
                {
                    if (currentSave.AllIfDepth - currentSave.TrueIfDepth == 1)
                    {
                        if (currentSave.AllIfDepth - currentSave.PassedIfDepth == 1)
                        {
                            currentSave.TrueIfDepth++;
                            currentSave.PassedIfDepth++;
                        }
                        currentSave.IfState.Pop();
                        currentSave.IfState.Push(IfState.Else);
                    }
                }
                else if (cmd.CommandName == "elseif")
                {
                    if (currentSave.AllIfDepth - currentSave.TrueIfDepth == 1)
                    {
                        if (currentSave.IfState.Peek() == IfState.Else)
                        {
                            error("elseif behind else, will be ignore");
                        }
                        else if (currentSave.AllIfDepth - currentSave.PassedIfDepth == 1)
                        {
                            string exp = "";
                            if (cmd.CommandParams.ContainsKey("exp"))
                            {
                                exp = cmd.CommandParams["exp"];
                            }
                            bool res = false;
                            if (exp.Length > 0)
                            {
                                try
                                {
                                    TJSVariable r = VM.Eval(exp);
                                    res = r.ToBoolean();
                                }
                                catch (Exception e)
                                {
                                    error(e.Message);
                                    return true;
                                }
                            }
                            if (res)
                            {
                                currentSave.TrueIfDepth++;
                                currentSave.PassedIfDepth++;
                            }
                            currentSave.IfState.Pop();
                            currentSave.IfState.Push(IfState.ElseIf);
                        }
                    }
                }
                else if (cmd.CommandName == "endif")
                {
                    if (currentSave.AllIfDepth == currentSave.IfState.Count)
                        currentSave.IfState.Pop();
                    currentSave.AllIfDepth--;
                    if (currentSave.TrueIfDepth > currentSave.AllIfDepth)
                        currentSave.TrueIfDepth = currentSave.AllIfDepth;
                    if (currentSave.PassedIfDepth > currentSave.AllIfDepth)
                        currentSave.PassedIfDepth = currentSave.AllIfDepth;
                }
                return true;
            }

            if (cmd.CommandName == "jump" || cmd.CommandName == "call")
            {
                string script = null;
                string label = null;
                if (!cmd.CommandParams.ContainsKey("storage") && !cmd.CommandParams.ContainsKey("target"))
                {
                    error("storage and target cannot both be null");
                    return true;
                }
                if (cmd.CommandParams.ContainsKey("storage"))
                {
                    script = cmd.CommandParams["storage"];
                }
                if (cmd.CommandParams.ContainsKey("target"))
                {
                    label = cmd.CommandParams["target"];
                    if (label.StartsWith("*"))
                        label = label.Substring(1);
                }
                if (cmd.CommandName == "jump")
                    Jump(script, label);
                else
                    Call(script, label);
                return true;
            }
            else if (cmd.CommandName == "return")
            {
                int times = 1;
                if (cmd.CommandParams.ContainsKey("times"))
                {
                    int.TryParse(cmd.CommandParams["times"], out times);
                }
                Return(times);
                return true;
            }
            else if (cmd.CommandName == "emb")
            {
                string exp = "";
                if (cmd.CommandParams.ContainsKey("exp"))
                {
                    exp = cmd.CommandParams["exp"];
                }
                if (exp.Length > 0)
                {
                    try
                    {
                        TJSVariable r = VM.Eval(exp);
                        string res = r.ToString();
                        MacroBuffer buf = new MacroBuffer();
                        buf.MacroString = res;
                        buf.Offset = 0;
                        currentCall.buffer.Add(buf);
                    }
                    catch (Exception e)
                    {
                        error(e.Message);
                        return true;
                    }
                }
                return true;
            }
            else if (cmd.CommandName == "eval")
            {
                string exp = "";
                if (cmd.CommandParams.ContainsKey("exp"))
                {
                    exp = cmd.CommandParams["exp"];
                }
                if (exp.Length > 0)
                {
                    try
                    {
                        VM.Eval(exp);
                    }
                    catch (Exception e)
                    {
                        error(e.Message);
                        return true;
                    }
                }
                return true;
            }
            else if (cmd.CommandName == "if")
            {
                string exp = "";
                if (cmd.CommandParams.ContainsKey("exp"))
                {
                    exp = cmd.CommandParams["exp"];
                }
                bool res = false;
                if (exp.Length > 0)
                {
                    try
                    {
                        TJSVariable r = VM.Eval(exp);
                        res = r.ToBoolean();
                    }
                    catch (Exception e)
                    {
                        error(e.Message);
                        return true;
                    }
                }
                currentSave.AllIfDepth++;
                currentSave.IfState.Push(IfState.If);
                if (res)
                {
                    currentSave.TrueIfDepth++;
                    currentSave.PassedIfDepth++;
                }
                return true;
            }
            else if (cmd.CommandName == "elseif")
            {
                if (currentSave.AllIfDepth <= 0)
                {
                    error("elseif without if");
                    return true;
                }
                currentSave.TrueIfDepth--;
                if (currentSave.IfState.Peek() == IfState.Else)
                {
                    error("elseif behind else, will be ignore");
                }
                else
                {
                    currentSave.IfState.Pop();
                    currentSave.IfState.Push(IfState.ElseIf);
                }
            }
            else if (cmd.CommandName == "else")
            {
                if (currentSave.AllIfDepth <= 0)
                {
                    error("else without if");
                    return true;
                }
                currentSave.TrueIfDepth--;
                currentSave.IfState.Pop();
                currentSave.IfState.Push(IfState.Else);
            }
            else if (cmd.CommandName == "endif")
            {
                if (currentSave.AllIfDepth <= 0)
                {
                    error("endif without if");
                    return true;
                }
                currentSave.AllIfDepth--;
                if (currentSave.TrueIfDepth > currentSave.AllIfDepth)
                    currentSave.TrueIfDepth = currentSave.AllIfDepth;
                if (currentSave.PassedIfDepth > currentSave.AllIfDepth)
                    currentSave.PassedIfDepth = currentSave.AllIfDepth;
                currentSave.IfState.Pop();
            }
            else if (cmd.CommandName == "macro")
            {
                if (currentCall.buffer.Count > 0)
                {
                    error("macro define cannot nested in another macro define");
                    return true;
                }
                string name = "";
                if (cmd.CommandParams.ContainsKey("name"))
                {
                    name = cmd.CommandParams["name"];
                }
                if (name.Length == 0)
                {
                    error("macro must have a name");
                }
                bool valid = true;
                for (int i = 0; i < name.Length; i++)
                {
                    if (char.IsLetterOrDigit(name[i]) || name[i] == '_' || name[i] > 127)
                    {
                        if (i == 0 && char.IsDigit(name[i]))
                        {
                            valid = false;
                            break;
                        }
                    }
                    else
                    {
                        valid = false;
                        break;
                    }
                }
                if (!valid)
                {
                    error($"macro name ({name}) is illegal");
                }
                currentMacro = new MacroInfo();
                currentMacroValid = valid;
                currentMacro.MacroName = name;
            }
            else if (cmd.CommandName == "endmacro")
            {
                error("endmacro without macro");
                return true;
            }
            else if (cmd.CommandName == "erasemacro")
            {
                string name = "";
                if (cmd.CommandParams.ContainsKey("name"))
                {
                    name = cmd.CommandParams["name"];
                }
                macros.Remove(name);
            }
            else if (cmd.CommandName == "macropop")
            {
                if (currentCall.buffer.Count > 0)
                {
                    currentCall.buffer.RemoveAt(currentCall.buffer.Count - 1);
                }
                else
                {
                    error("meet macropop but not in a macro context, DO NOT write this command manually");
                }
            }
            else if (cmd.CommandName == "iscript")
            {
                string exp = "";
                while (true)
                {
                    string text = getCurLine();
                    if (text == null)
                    {
                        error("[iscript] need [endscript]");
                    }
                    proceedOffset(text.Length);
                    text = text.TrimStart();
                    if (text.StartsWith("[endscript]"))
                        break;
                    exp += text;
                    exp += '\n';
                }
                try
                {
                    VM.Eval(exp);
                }
                catch (Exception e)
                {
                    error(e.Message);
                    return true;
                }
            }
            else if (macros.ContainsKey(cmd.CommandName))
            {
                MacroBuffer buf = new MacroBuffer();
                buf.MacroParams = cmd.CommandParams;
                buf.MacroString = macros[cmd.CommandName].MacroLine;
                buf.Offset = 0;
                currentCall.buffer.Add(buf);
            }
            else
            {
                return false;
            }

            return true;
        }

        string getCurLine()
        {
            if (currentCall.buffer.Count > 0)
            {
                var c = currentCall.buffer[currentCall.buffer.Count - 1];
                var str = c.MacroString.Substring(c.Offset);
                if (c.Offset == 0 || c.MacroString[c.Offset - 1] == '\n')
                    isLineHead = true;
                else
                    isLineHead = false;
                var npos = str.IndexOf('\n');
                if (npos >= 0)
                    return str.Substring(0, npos);
                else
                    return str;
            }
            isLineHead = currentCall.LineOffset == 0;
            if (currentCall.LineNumber < currentScriptCache.lines.Length)
                return currentScriptCache.lines[currentCall.LineNumber].Substring(currentCall.LineOffset);
            else
                return null;
        }

        public string GetPrevLine()
        {
            if (currentCall.buffer.Count > 0)
            {
                var c = currentCall.buffer[currentCall.buffer.Count - 1];
                var str = c.MacroString.Substring(c.PrevOffset);
                var npos = str.IndexOf('\n');
                if (npos >= 0)
                    return str.Substring(0, npos);
                else
                    return str;
            }
            if (currentCall.PrevLineNumber < currentScriptCache.lines.Length)
                return currentScriptCache.lines[currentCall.PrevLineNumber].Substring(currentCall.PrevLineOffset);
            else
                return null;
        }

        int skipToChar(string text, string end, int start = 0)
        {
            int i = start;
            bool instr = false;
            while (i < text.Length)
            {
                if (text[i] == '\"')
                {
                    instr = !instr;
                    i++;
                    continue;
                }
                if (text[i] == '\\')
                {
                    i++;
                    if (i < text.Length)
                    {
                        if (text[i] == 'x')
                            i += 2;
                        else if (char.IsDigit(text[i]))
                            i += 2;
                        else
                            i++;
                    }
                    continue;
                }
                if (end.IndexOf(text[i]) >= 0 && !instr)
                    return i;
                i++;
            }
            return i;
        }

        void proceedOffset(int n)
        {
            if (currentCall.buffer.Count > 0)
            {
                var c = currentCall.buffer[currentCall.buffer.Count - 1];
                c.PrevOffset = c.Offset;
                c.Offset += n;
                while (c.Offset < c.MacroString.Length && c.MacroString[c.Offset] == '\n')
                    c.Offset++;
                while (c.Offset >= c.MacroString.Length)
                {
                    currentCall.buffer.Remove(c);
                    if (currentCall.buffer.Count == 0)
                        return;
                    c = currentCall.buffer[currentCall.buffer.Count - 1];
                }
                return;
            }
            var curline = currentScriptCache.lines[currentCall.LineNumber];
            currentCall.PrevLineOffset = currentCall.LineOffset;
            currentCall.PrevLineNumber = currentCall.LineNumber;
            currentCall.LineOffset += n;
            if (currentCall.LineOffset >= curline.Length)
            {
                currentCall.LineOffset = 0;
                currentCall.LineNumber++;
            }
        }

        void newCallInfo()
        {
            currentSave.CallStack.Add(new CallStackInfo());
        }

        int searchLabel(string label = null)
        {
            if (label == null)
                return 0;
            if (currentScriptCache.labelmap.ContainsKey(label))
                return currentScriptCache.labelmap[label];
            if (currentScriptCache.parseOver)
                return -1;
            for (int i = currentScriptCache.parseLineNumber; i < currentScriptCache.lines.Length; i++)
            {
                var text = currentScriptCache.lines[i];
                if (text.Length > 0 && text[0] == '*')
                {
                    var labelname = text.Substring(1);
                    bool isSave = false;
                    if (labelname.EndsWith("|"))
                    {
                        labelname = labelname.Substring(0, labelname.Length - 1);
                        isSave = true;
                    }
                    int j = 1;
                    var newlabelname = labelname;
                    while (currentScriptCache.labelmap.ContainsKey(newlabelname))
                    {
                        j++;
                        newlabelname = labelname + ":" + j.ToString();
                    }
                    currentScriptCache.labelmap[newlabelname] = i;
                    currentScriptCache.labelsaveinfo[newlabelname] = isSave;
                    if (newlabelname == label)
                    {
                        currentScriptCache.parseLineNumber = i;
                        if (i >= currentScriptCache.lines.Length - 1)
                            currentScriptCache.parseOver = true;
                        return i;
                    }
                }
            }
            return -1;
        }

        void error(string msg)
        {
            ErrorMessage = $"Error:{msg} at {CurrentScript}, line {PrevLineNumber}, when parse {GetPrevLine()}";
            UnityEngine.Debug.LogError(ErrorMessage);
            //skip to next line
            currentCall.LineNumber++;
            currentCall.LineOffset = 0;
        }
    }
}