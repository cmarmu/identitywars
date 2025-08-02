
using System;
using System.Collections.Generic;

[Serializable]
public class QuestionEntry
{
    public string question;
    public List<string> choices;
    public int correct;
    public string level;
}
