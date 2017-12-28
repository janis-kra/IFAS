var FeedParser = require('feedparser');
var request = require('request'); // for fetching the feed

const options = {
  url: 'http://localhost:2113/streams/$all',
  headers: {
    'Accept': 'application/atom+xml'
  }
}

var req = request(options).auth('admin', 'changeit', false);
var feedparser = new FeedParser({
  normalize: false
});

req.on('error', function (error) {
  console.error(error)
});

req.on('response', function (res) {
  var stream = this; // `this` is `req`, which is a stream

  if (res.statusCode !== 200) {
    this.emit('error', new Error('Bad status code'));
  }
  else {
    stream.pipe(feedparser);
  }
});

feedparser.on('error', function (error) {
  console.error(error)
});

feedparser.on('meta', function (meta) {
  console.log('===== %s =====', JSON.stringify(meta));
})

let counter = 0;
feedparser.on('readable', function () {
  // This is where the action is!
  var stream = this; // `this` is `feedparser`, which is a stream
  var meta = this.meta; // **NOTE** the "meta" is always available in the context of the feedparser instance
  var item;

  while (item = stream.read()) {
    counter++;
    console.log(counter + ': ' + item['atom:id']['#']);
    
    console.log(item);
    /*
      curl -XPUT 'localhost:9200/twitter/tweet/1?pretty' -H 'Content-Type: application/json' -d'
{
    "user" : "kimchy",
    "post_date" : "2009-11-15T14:12:12",
    "message" : "trying out Elasticsearch"
}
'

    */
    // request.put({
    //   url: 'localhost:9200/streams'
    // })
  }
});