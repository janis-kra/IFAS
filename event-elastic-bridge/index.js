const FeedParser = require("feedparser");
const got = require("got");

require('dotenv').config();

const options = {
  headers: {
    Accept: "application/atom+xml"
  },
  auth: `${process.env.user}:${process.env.pw}`
};

function initFeedparser (resolve, reject, rel) {
  const feedparser = new FeedParser({
    normalize: false
  });

  feedparser.on("error", function(error) {
    reject(error);
  });

  feedparser.on("meta", function(meta) {
    if (meta["atom:link"]) {
      const links = meta["atom:link"].map(link => link["@"]);
      let href;
      links.forEach(link => {
        if (link.rel === rel) {
          last = link.href;
        }
      });
      resolve(last);
    } else {
      resolve();
    }
  });

  return feedparser;
}

async function fetchLastPage(url) {
  return new Promise((resolve, reject) => {
    const feedparser = initFeedparser(resolve, reject, "last");
    got.stream(url, options).pipe(feedparser);
  });
}

async function read(url) {
  return new Promise((resolve, reject) => {
    const feedparser = initFeedparser(resolve, reject, "previous");

    feedparser.on("readable", function() {
      const parser = this; // `this` is `feedparser`, which is a stream
      const meta = this.meta; // **NOTE** the "meta" is always available in the context of the feedparser instance
      const output = process.stdout; // TODO: Change to got.stream('localhost:9200/streams', ...) --> Elasticsearch

      parser.stream.pipe(output); // parser.stream is the nodejs stream.Duplex and thus stream.Readable
    });

    got.stream(url, options).pipe(feedparser);
  });
}

async function readUiData () {
  const startUrl = "http://localhost:2113/streams/ui-data";
  let url = await fetchLastPage(startUrl);
  const count = Number.MAX_SAFE_INTEGER;
  for (let i = 0; i < count && url; i++) {
    try {
      url = await read(url);
    } catch (error) {
      console.error(error);
    }
  }
};

readUiData();

// var req = request(options).auth("admin", "changeit", false);

// req.on("error", function(error) {
//   console.error(error);
// });

// req.on("response", function(res) {
//   var stream = this; // `this` is `req`, which is a stream

//   if (res.statusCode !== 200) {
//     this.emit("error", new Error("Bad status code"));
//   } else {
//     stream.pipe(feedparser);
//   }
// });

// feedparser.on("error", function(error) {
//   console.error(error);
// });

// feedparser.on("meta", function(meta) {
//   // console.log(JSON.stringify(meta));
//   if (meta["atom:link"]) {
//     const links = meta["atom:link"].map(link => link["@"]);
//     links.forEach(link => {
//       if (link.rel === "next") {
//         console.log(`read next link: ${link.href}`);
//       }
//     });
//   }
// });

// let counter = 0;
// feedparser.on('readable', function () {
//   // This is where the action is!
//   var stream = this; // `this` is `feedparser`, which is a stream
//   var meta = this.meta; // **NOTE** the "meta" is always available in the context of the feedparser instance
//   var item;

//   while (item = stream.read()) {
//     counter++;
//     console.log(counter + ': ' + item['atom:id']['#']);

//     console.log(item);
//     /*
//       curl -XPUT 'localhost:9200/twitter/tweet/1?pretty' -H 'Content-Type: application/json' -d'
// {
//     "user" : "kimchy",
//     "post_date" : "2009-11-15T14:12:12",
//     "message" : "trying out Elasticsearch"
// }
// '

//     */
//     // request.put({
//     //   url: 'localhost:9200/streams'
//     // })
//   }
// });
