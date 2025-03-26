using System;
using UnityEngine;

public class Opdracht
{
    public string Title { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }

    public Opdracht(string title, DateTime dueDate)
    {
        Title = title;
        DueDate = dueDate;
        IsCompleted = false;
    }
}