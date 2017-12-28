const csv = require("fast-csv");
const fs = require("fs");
const got = require("got");

const url = "http://localhost:2113/streams/ui-data";

/* UI DETAILS */

(function() {
  const file = "data/ui_details-small.csv";
  const csvStream = csv({ headers: true })
    .on("data", function(data) {
      got.post(
        url,
        {
          body: JSON.stringify(data),
          headers: {
            "Content-Type": "application/json",
            "ES-EventType": "UIData-Created"
          }
        },
        res => console.log(`Posted data: ${data["UI Number"]}`)
      );
    })
    .on("end", function() {
      console.log(`Done posting all data from ${file} to event store at ${url}`);
    });

  const fileStream = fs.createReadStream(file);
  fileStream.pipe(csvStream);
})();
