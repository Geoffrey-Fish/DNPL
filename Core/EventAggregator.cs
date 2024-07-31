using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNPL.Core;
public class EventAggregator {
	private static EventAggregator _instance;
	private Dictionary<string,List<Action<object>>> _events;

	private EventAggregator() {
		_events=new Dictionary<string,List<Action<object>>>();
	}

	public static EventAggregator Instance {
		get {
			if(_instance==null) {
				_instance=new EventAggregator();
			}
			return _instance;
		}
	}

	public void Publish<T>(string eventName,T payload) {
		if(_events.ContainsKey(eventName)) {
			_events[eventName].ForEach(subscriber => subscriber(payload));
		}
	}

	public void Subscribe(string eventName,Action<object> action) {
		if(!_events.ContainsKey(eventName)) {
			_events[eventName]=new List<Action<object>>();
		}
		_events[eventName].Add(action);
	}

	public void Unsubscribe(string eventName,Action<object> action) {
		if(_events.ContainsKey(eventName)) {
			_events[eventName].Remove(action);
		}
	}
}
