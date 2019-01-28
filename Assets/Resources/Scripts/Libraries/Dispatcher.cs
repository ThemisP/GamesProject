using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Dispatcher {

    private Queue<Action> RunOnMainThread = new Queue<Action>();

    public void Execute() {
        if (RunOnMainThread.Count > 0) {
            lock (RunOnMainThread) {
                Action s = RunOnMainThread.Dequeue();
                s();
            }
        } 
    }

    public void CallFunctionFromAnotherThread(Action functionName) {
        lock (RunOnMainThread) {
            RunOnMainThread.Enqueue(functionName);
        }
    }
}
