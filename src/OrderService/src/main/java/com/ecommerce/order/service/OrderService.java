package com.ecommerce.order.service;

import com.ecommerce.order.dto.CreateOrderRequest;
import com.ecommerce.order.model.*;
import com.ecommerce.order.repository.OrderRepository;
import com.ecommerce.order.event.OrderEventProducer;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.Optional;

/**
 * Order Service
 * 
 * Business logic for order management.
 * Handles order creation, updates, and querying.
 */
@Service
@RequiredArgsConstructor
@Slf4j
public class OrderService {

    private final OrderRepository orderRepository;
    private final OrderEventProducer orderEventProducer;

    /**
     * Create a new order
     */
    @Transactional
    public Order createOrder(CreateOrderRequest request) {
        log.info("Creating order for customer: {}", request.getCustomerId());

        // Build order entity
        Order order = Order.builder()
                .customerId(request.getCustomerId())
                .status(OrderStatus.PENDING)
                .shippingAddress(request.getShippingAddress())
                .billingAddress(request.getBillingAddress())
                .paymentMethod(request.getPaymentMethod())
                .paymentStatus(PaymentStatus.PENDING)
                .notes(request.getNotes())
                .build();

        // Add order items
        request.getItems().forEach(itemRequest -> {
            OrderItem item = OrderItem.builder()
                    .productId(itemRequest.getProductId())
                    .productName(itemRequest.getProductName())
                    .productSku(itemRequest.getProductSku())
                    .quantity(itemRequest.getQuantity())
                    .price(itemRequest.getPrice())
                    .build();
            order.addOrderItem(item);
        });

        // Calculate total
        order.calculateTotal();

        // Save order
        Order savedOrder = orderRepository.save(order);

        log.info("Order created successfully: {}", savedOrder.getId());
        
        // Send order placed event to Kafka
        orderEventProducer.sendOrderPlacedEvent(savedOrder);

        return savedOrder;
    }

    /**
     * Get order by ID
     */
    public Optional<Order> getOrderById(String id) {
        log.debug("Fetching order by ID: {}", id);
        return orderRepository.findById(id);
    }

    /**
     * Get orders by customer ID
     */
    public List<Order> getOrdersByCustomerId(String customerId) {
        log.debug("Fetching orders for customer: {}", customerId);
        return orderRepository.findByCustomerId(customerId);
    }

    /**
     * Get orders by customer ID with pagination
     */
    public Page<Order> getOrdersByCustomerId(String customerId, Pageable pageable) {
        log.debug("Fetching orders for customer: {} with pagination", customerId);
        return orderRepository.findByCustomerId(customerId, pageable);
    }

    /**
     * Get all orders with pagination
     */
    public Page<Order> getAllOrders(Pageable pageable) {
        log.debug("Fetching all orders with pagination");
        return orderRepository.findAll(pageable);
    }

    /**
     * Get orders by status
     */
    public List<Order> getOrdersByStatus(OrderStatus status) {
        log.debug("Fetching orders by status: {}", status);
        return orderRepository.findByStatus(status);
    }

    /**
     * Update order status
     */
    @Transactional
    public Order updateOrderStatus(String orderId, OrderStatus newStatus) {
        log.info("Updating order {} status to {}", orderId, newStatus);
        
        Order order = orderRepository.findById(orderId)
                .orElseThrow(() -> new RuntimeException("Order not found: " + orderId));
        
        order.setStatus(newStatus);
        return orderRepository.save(order);
    }

    /**
     * Cancel order
     */
    @Transactional
    public Order cancelOrder(String orderId) {
        log.info("Cancelling order: {}", orderId);
        
        Order order = orderRepository.findById(orderId)
                .orElseThrow(() -> new RuntimeException("Order not found: " + orderId));
        
        // Only allow cancellation of pending orders
        if (order.getStatus() != OrderStatus.PENDING) {
            throw new IllegalStateException("Only pending orders can be cancelled");
        }
        
        order.setStatus(OrderStatus.CANCELLED);
        return orderRepository.save(order);
    }

    /**
     * Get customer order statistics
     */
    public OrderStatistics getCustomerOrderStatistics(String customerId) {
        Long orderCount = orderRepository.countByCustomerId(customerId);
        Double totalSpent = orderRepository.getTotalSpentByCustomer(customerId);
        
        return OrderStatistics.builder()
                .customerId(customerId)
                .orderCount(orderCount != null ? orderCount : 0)
                .totalSpent(totalSpent != null ? totalSpent : 0.0)
                .build();
    }
}

/**
 * Order Statistics DTO
 */
@lombok.Data
@lombok.Builder
class OrderStatistics {
    private String customerId;
    private Long orderCount;
    private Double totalSpent;
}
