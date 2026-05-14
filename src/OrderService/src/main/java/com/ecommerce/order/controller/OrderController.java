package com.ecommerce.order.controller;

import com.ecommerce.order.dto.CreateOrderRequest;
import com.ecommerce.order.model.*;
import com.ecommerce.order.service.CartService;
import com.ecommerce.order.service.OrderService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.data.domain.Sort;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.math.BigDecimal;
import java.util.List;

/**
 * Order Controller
 * 
 * REST API for order management.
 * Provides endpoints for order and cart operations.
 */
@RestController
@RequestMapping("/api/orders")
@RequiredArgsConstructor
@Slf4j
public class OrderController {

    private final OrderService orderService;
    private final CartService cartService;

    /**
     * Create a new order
     */
    @PostMapping
    public ResponseEntity<Order> createOrder(@Valid @RequestBody CreateOrderRequest request) {
        log.info("Creating order for customer: {}", request.getCustomerId());
        Order order = orderService.createOrder(request);
        return ResponseEntity.status(HttpStatus.CREATED).body(order);
    }

    /**
     * Get order by ID
     */
    @GetMapping("/{id}")
    public ResponseEntity<Order> getOrder(@PathVariable String id) {
        return orderService.getOrderById(id)
                .map(ResponseEntity::ok)
                .orElse(ResponseEntity.notFound().build());
    }

    /**
     * Get orders by customer
     */
    @GetMapping("/customer/{customerId}")
    public ResponseEntity<List<Order>> getOrdersByCustomer(@PathVariable String customerId) {
        List<Order> orders = orderService.getOrdersByCustomerId(customerId);
        return ResponseEntity.ok(orders);
    }

    /**
     * Get orders by customer with pagination
     */
    @GetMapping("/customer/{customerId}/page")
    public ResponseEntity<Page<Order>> getOrdersByCustomerPage(
            @PathVariable String customerId,
            @RequestParam(defaultValue = "0") int page,
            @RequestParam(defaultValue = "10") int size) {
        Pageable pageable = PageRequest.of(page, size, Sort.by("createdAt").descending());
        Page<Order> orders = orderService.getOrdersByCustomerId(customerId, pageable);
        return ResponseEntity.ok(orders);
    }

    /**
     * Get all orders with pagination
     */
    @GetMapping
    public ResponseEntity<Page<Order>> getAllOrders(
            @RequestParam(defaultValue = "0") int page,
            @RequestParam(defaultValue = "10") int size) {
        Pageable pageable = PageRequest.of(page, size, Sort.by("createdAt").descending());
        Page<Order> orders = orderService.getAllOrders(pageable);
        return ResponseEntity.ok(orders);
    }

    /**
     * Get orders by status
     */
    @GetMapping("/status/{status}")
    public ResponseEntity<List<Order>> getOrdersByStatus(@PathVariable OrderStatus status) {
        List<Order> orders = orderService.getOrdersByStatus(status);
        return ResponseEntity.ok(orders);
    }

    /**
     * Update order status
     */
    @PutMapping("/{id}/status")
    public ResponseEntity<Order> updateOrderStatus(
            @PathVariable String id,
            @RequestParam OrderStatus status) {
        Order order = orderService.updateOrderStatus(id, status);
        return ResponseEntity.ok(order);
    }

    /**
     * Cancel order
     */
    @DeleteMapping("/{id}")
    public ResponseEntity<Order> cancelOrder(@PathVariable String id) {
        Order order = orderService.cancelOrder(id);
        return ResponseEntity.ok(order);
    }

    // ==================== Cart Endpoints ====================

    /**
     * Get cart for customer
     */
    @GetMapping("/cart/{customerId}")
    public ResponseEntity<List<CartItem>> getCart(@PathVariable String customerId) {
        List<CartItem> cart = cartService.getCart(customerId);
        return ResponseEntity.ok(cart);
    }

    /**
     * Add item to cart
     */
    @PostMapping("/cart/{customerId}")
    public ResponseEntity<Void> addToCart(
            @PathVariable String customerId,
            @RequestBody CartItem item) {
        cartService.addToCart(customerId, item);
        return ResponseEntity.ok().build();
    }

    /**
     * Update cart item quantity
     */
    @PutMapping("/cart/{customerId}/item/{productId}")
    public ResponseEntity<Void> updateQuantity(
            @PathVariable String customerId,
            @PathVariable String productId,
            @RequestParam Integer quantity) {
        cartService.updateQuantity(customerId, productId, quantity);
        return ResponseEntity.ok().build();
    }

    /**
     * Remove item from cart
     */
    @DeleteMapping("/cart/{customerId}/item/{productId}")
    public ResponseEntity<Void> removeFromCart(
            @PathVariable String customerId,
            @PathVariable String productId) {
        cartService.removeFromCart(customerId, productId);
        return ResponseEntity.noContent().build();
    }

    /**
     * Clear cart
     */
    @DeleteMapping("/cart/{customerId}")
    public ResponseEntity<Void> clearCart(@PathVariable String customerId) {
        cartService.clearCart(customerId);
        return ResponseEntity.noContent().build();
    }

    /**
     * Get cart total
     */
    @GetMapping("/cart/{customerId}/total")
    public ResponseEntity<BigDecimal> getCartTotal(@PathVariable String customerId) {
        BigDecimal total = cartService.getCartTotal(customerId);
        return ResponseEntity.ok(total);
    }

    /**
     * Get cart item count
     */
    @GetMapping("/cart/{customerId}/count")
    public ResponseEntity<Integer> getCartItemCount(@PathVariable String customerId) {
        int count = cartService.getCartItemCount(customerId);
        return ResponseEntity.ok(count);
    }
}
