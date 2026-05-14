package com.ecommerce.order.service;

import com.ecommerce.order.model.CartItem;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.datatype.jsr310.JavaTimeModule;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.data.redis.core.RedisTemplate;
import org.springframework.stereotype.Service;

import java.math.BigDecimal;
import java.time.Duration;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * Cart Service
 * 
 * Manages shopping cart sessions using Redis.
 * Implements TTL (Time To Live) for cart expiration.
 */
@Service
@RequiredArgsConstructor
@Slf4j
public class CartService {

    private final RedisTemplate<String, Object> redisTemplate;
    private final ObjectMapper objectMapper;

    // Cart TTL - 24 hours
    private static final Duration CART_TTL = Duration.ofHours(24);
    
    // Cart key prefix
    private static final String CART_PREFIX = "cart:";

    /**
     * Get cart for a customer
     */
    public List<CartItem> getCart(String customerId) {
        String cartKey = CART_PREFIX + customerId;
        
        try {
            Object cached = redisTemplate.opsForValue().get(cartKey);
            if (cached == null) {
                return new ArrayList<>();
            }
            
            // Convert cached object to List
            return objectMapper.readValue(
                objectMapper.writeValueAsString(cached),
                objectMapper.getTypeFactory().constructCollectionType(List.class, CartItem.class)
            );
        } catch (Exception e) {
            log.error("Error getting cart for customer: {}", customerId, e);
            return new ArrayList<>();
        }
    }

    /**
     * Add item to cart
     */
    public void addToCart(String customerId, CartItem item) {
        List<CartItem> cart = getCart(customerId);
        item.calculateSubtotal();
        
        // Check if product already in cart
        boolean found = false;
        for (CartItem existing : cart) {
            if (existing.getProductId().equals(item.getProductId())) {
                // Update quantity
                existing.setQuantity(existing.getQuantity() + item.getQuantity());
                existing.calculateSubtotal();
                found = true;
                break;
            }
        }
        
        // Add new item if not found
        if (!found) {
            cart.add(item);
        }
        
        saveCart(customerId, cart);
        log.info("Added item {} to cart for customer: {}", item.getProductId(), customerId);
    }

    /**
     * Update item quantity in cart
     */
    public void updateQuantity(String customerId, String productId, Integer quantity) {
        List<CartItem> cart = getCart(customerId);
        
        for (CartItem item : cart) {
            if (item.getProductId().equals(productId)) {
                item.setQuantity(quantity);
                item.calculateSubtotal();
                break;
            }
        }
        
        saveCart(customerId, cart);
        log.info("Updated quantity for product {} in cart for customer: {}", productId, customerId);
    }

    /**
     * Remove item from cart
     */
    public void removeFromCart(String customerId, String productId) {
        List<CartItem> cart = getCart(customerId);
        cart.removeIf(item -> item.getProductId().equals(productId));
        
        saveCart(customerId, cart);
        log.info("Removed item {} from cart for customer: {}", productId, customerId);
    }

    /**
     * Clear cart
     */
    public void clearCart(String customerId) {
        String cartKey = CART_PREFIX + customerId;
        redisTemplate.delete(cartKey);
        log.info("Cleared cart for customer: {}", customerId);
    }

    /**
     * Get cart total
     */
    public BigDecimal getCartTotal(String customerId) {
        List<CartItem> cart = getCart(customerId);
        
        return cart.stream()
                .map(CartItem::getSubtotal)
                .reduce(BigDecimal.ZERO, BigDecimal::add);
    }

    /**
     * Get cart item count
     */
    public int getCartItemCount(String customerId) {
        List<CartItem> cart = getCart(customerId);
        
        return cart.stream()
                .mapToInt(CartItem::getQuantity)
                .sum();
    }

    /**
     * Save cart to Redis with TTL
     */
    private void saveCart(String customerId, List<CartItem> cart) {
        String cartKey = CART_PREFIX + customerId;
        
        try {
            redisTemplate.opsForValue().set(cartKey, cart, CART_TTL);
        } catch (Exception e) {
            log.error("Error saving cart for customer: {}", customerId, e);
            throw new RuntimeException("Failed to save cart", e);
        }
    }
}
