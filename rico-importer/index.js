const csv = require("fast-csv");
const fs = require("fs");
const got = require("got");

require("dotenv").config();

const url = `http://localhost:2113/streams/${process.env.INDEX}`;

/* UI DETAILS */

(function() {
  let i = 0;
  const limit = Number.parseInt(process.env.LIMIT) || Number.MAX_SAFE_INTEGER;
  const file = `data/${process.env.INDEX}.csv`;
  const csvStream = csv({ headers: true })
    .on("data", function(data) {
      if (i < limit) {
        got.post(
          url,
          {
            body: JSON.stringify(data),
            headers: {
              "Content-Type": "application/json",
              "ES-EventType": `${process.env.INDEX}-created`
            }
          },
          res => console.log(`Posted data: ${data}`)
        );
        i++;
      }
    })
    .on("end", function() {
      console.log(
        `Done posting ${i} lines from ${file} to event store at ${url}`
      );
    });

  const fileStream = fs.createReadStream(file);
  fileStream.pipe(csvStream);
})();
