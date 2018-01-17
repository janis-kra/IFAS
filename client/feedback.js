var url = "http://127.0.0.1:2113/streams/analytics";
function goleft() {
  feedback({ direction: "left" });
}
function goright() {
  feedback({ direction: "right" });
}
function feedback(data) {
  data.timestamp = Date.now();
  var xhr = new XMLHttpRequest();
  xhr.open('post', url);
  xhr.setRequestHeader("Content-type", "application/json");
  xhr.setRequestHeader("ES-EventType", "analytics-created");
  xhr.setRequestHeader("ES-EventId", uuidv4());
  xhr.send(JSON.stringify(data));
}
function uuidv4() {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
    var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
    return v.toString(16);
  });
}
