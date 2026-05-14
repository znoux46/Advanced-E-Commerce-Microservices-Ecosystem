package com.ecommerce.order.event;

import com.ecommerce.order.model.Order;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.kafka.core.KafkaTemplate;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.util.HashMap;
import java.util.Map;

/**
 * Order Event Producer
 * 
 * Sends order events to Apache Kafka.
 * When an order is placed, it sends an OrderPlaced event to the Kafka topic.
 */
@Service
@RequiredArgsConstructor
@Slf4j
public class OrderEventProducer {

    private final KafkaTemplate<String, Object> kafkaTemplate;
    
    // Kafka topic for order events
    private static final String ORDER_TOPIC = "order-placed";

    /**
     * Send OrderPlaced event to Kafka
     * 
     * This event is consumed by Inventory Service to automatically deduct stock.
     */
    public void sendOrderPlacedEvent(Order order) {
        log.info("Sending OrderPlaced event to Kafka for order: {}", order.getId());
        
        try {
            // Create event payload
            Map<String, Object> event = new HashMap<>();
            event.put("eventType", "OrderPlaced");
            event.put("orderId", order.getId());
            event.put("customerId", order.getCustomerId());
            event.put("status", order.getStatus().name());
            event.put("totalAmount", order.getTotalAmount());
            event.put("timestamp", LocalDateTime.now().toString());
            
            // Add order items
            Map<String, Object> items = new HashMap<>();
            order.getOrderItems().forEach(item -> {
                Map<String, Object> itemData = new HashMap<>();
                itemData.put("productId", item.getProductId());
                itemData.put("productName", item.getProductName());
                itemData.put("quantity", item.getQuantity());
                itemData.put("price", item.getPrice());
                items.put(item.getProductId(), itemData);
            });
            event.put("items", items);
            
            // Send to Kafka
            kafkaTemplate.send(ORDER_TOPIC, order.getId(), event);
            log.info("OrderPlaced event sent successfully for order: {}", order.getId());
            
        } catch (Exception e) {
            log.error("Failed to send OrderPlaced event to Kafka", e);
            throw new RuntimeException("Failed to send OrderPlaced event", e);
        }
    }

    /**
     * Send OrderCancelled event to Kafka
     */
    public void sendOrderCancelledEvent(Order order) {
        log.info("Sending OrderCancelled event to Kafka for order: {}", order.getId());
        
        Map<String, Object> event = new HashMap<>();
        event.put("eventType", "OrderCancelled");
        event.put("orderId", order.getId());
        event.put("customerId", order.getCustomerId());
        event.put("timestamp", LocalDateTime.now().toString());
        
        kafkaTemplate.send(ORDER_TOPIC, order.getId(), event);
    }
}
