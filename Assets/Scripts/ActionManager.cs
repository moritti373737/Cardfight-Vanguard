using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class ActionManager : SingletonMonoBehaviour<ActionManager>
{
    public ReactiveCollection<ActionData> ActionHistory { get; private set; } = new ReactiveCollection<ActionData>();

    void Start()
    {
        ActionHistory.ObserveCountChanged().Subscribe(count => Debug.Log(count));
    }

    void Update()
    {

    }
    /// <summary>
    /// アプリケーションが終了する前に呼び出されます
    /// </summary>
    private void OnApplicationQuit()
    {
        ActionHistory.ToList().ForEach(t => Debug.Log($"{t.Title}, {t.FighterID} || {t.Source?.ToString()} = {t.Card} => {t.Target?.ToString()}"));
    }
}

public class ActionData
{
    public string Title { get; set; }
    public FighterID FighterID { get; set; }
    public Card Card { get; set; }
    public object Source { get; set; }
    public object Target { get; set; }
    public ActionData(string title, FighterID fighterID, Card card, object source, object target)
    {
        Title = title;
        FighterID = fighterID;
        Card = card;
        Source = source;
        Target = target ?? source;
    }
}