package de.janiskrasemann.eventelasticbridge;

import akka.actor.ActorSystem;
import eventstore.Content;
import eventstore.Event;
import eventstore.Settings;
import eventstore.SubscriptionObserver;
import eventstore.j.EsConnection;
import eventstore.j.EsConnectionFactory;
import eventstore.j.SettingsBuilder;

import java.io.Closeable;
import java.net.InetSocketAddress;
import java.nio.charset.StandardCharsets;

public class SubscribeToAllExample {
    public static void main(String[] args) {
        final String streamName = System.getenv("STREAM_NAME");
        final String pw = System.getenv("PW");
        final String user = System.getenv("USER");

        final ActorSystem system = ActorSystem.create();
        final Settings settings = new SettingsBuilder()
                .address(new InetSocketAddress("127.0.0.1", 1113))
                .defaultCredentials(user, pw)
                .build();
        final EsConnection connection = EsConnectionFactory.create(system, settings);
        final SubscriptionObserver<Event> subscription = new SubscriptionObserver<Event>() {
          @Override
          public void onLiveProcessingStart(Closeable subscription) {
              system.log().info("live processing started");
          }

          @Override
          public void onEvent(Event event, Closeable subscription) {
            Content content = event.record().data().data();
            byte[] bytes = content.value().toArray();
            String eventData = new String(bytes, StandardCharsets.UTF_8);
            system.log().info(eventData);
          }

          @Override
          public void onError(Throwable e) {
              system.log().error(e.toString());
          }

          @Override
          public void onClose() {
              system.log().error("subscription closed");
          }
        };
        final Closeable closeable = connection.subscribeToStream(streamName, subscription, false, null);
    }
}