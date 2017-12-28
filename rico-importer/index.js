const csv = require("fast-csv");
const fs = require("fs");
const got = require("got");

/* UI DETAILS */

(function() {
  const csvStream = csv({ headers: true })
    .on("data", function(data) {
      console.log(data);
      // curl -i -d@/home/greg/myevent.txt "http://127.0.0.1:2113/streams/newstream" -H "Content-Type:application/json" -H "ES-EventType: SomeEvent" -H "ES-EventId: C322E299-CB73-4B47-97C5-5054F920746E"
      got.post(
        "http://localhost:2113/streams/ui-data",
        {
          body: JSON.stringify(data),
          headers: {
            "Content-Type": "application/json",
            "ES-EventType": "UIData-Created"
          }
        },
        res => console.log("done posting data " + data["UI Number"])
      );
    })
    .on("end", function() {
      console.log("done");
    });
  
  const fileStream = fs.createReadStream("data/ui_details-small.csv");
  fileStream.pipe(csvStream);
})();
