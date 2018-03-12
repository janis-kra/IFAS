const got = require("got");

const STANDARD_URL = "http://127.0.0.1:2113/streams";
const EXPERIMENT_SIZE = process.env.EXPERIMENT_SIZE || 150;
const STANDARD_STREAM = `performance-${EXPERIMENT_SIZE}`;
const CHUNK_SIZE = 100;

const screen = {
  height: 1080,
  width: 1900
};
const url = `${STANDARD_URL}/${STANDARD_STREAM}`;

function feedback(data, eventType) {
  const options = {
    headers: {}
  };
  data["@timestamp"] = new Date().toISOString();
  options.headers["Content-type"] = "application/json";
  options.headers["ES-EventType"] = eventType;
  options.headers["ES-EventId"] = uuidv4();
  options.body = JSON.stringify(data);

  return got.post(url, options);
}

function uuidv4() {
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, c => {
    const r = (Math.random() * 16) | 0;
    const v = c == "x" ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

(async () => {
  const beginning = Date.now();
  console.log(`Experiment begins with size ${EXPERIMENT_SIZE} at ${beginning}`);

  for (let i = 1; i <= EXPERIMENT_SIZE; i++) {
    const data = {
      click: {
        x: Math.random() * screen.width,
        y: Math.random() * screen.height
      },
      screen
    };
    if (i % CHUNK_SIZE === 0) {
      await feedback(data, "UserClicked");
      const progress = i / EXPERIMENT_SIZE * 100;
      console.log(`${progress}%`);
    } else {
      feedback(data, "UserClicked");
    }
  }

  const end = Date.now();
  const duration = (end - beginning) / 1000; // duration in seconds
  console.log(`Experiment ended with size ${EXPERIMENT_SIZE} at ${end}`);
  console.log(`Generation Duration: ${duration}s`);
})();
