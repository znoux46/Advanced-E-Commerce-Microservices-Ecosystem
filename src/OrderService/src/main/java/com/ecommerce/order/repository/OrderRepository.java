package com.ecommerce.order.repository;

import com.ecommerce.order.model.Order;
import com.ecommerce.order.model.OrderStatus;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

import java.util.List;

/**
 * Order Repository
 * 
 * JPA Repository for Order entity.
 * Provides database operations for orders.
 */
@Repository
public interface OrderRepository extends JpaRepository<Order, String> {

    /**
     * Find orders by customer ID
     */
    List<Order> findByCustomerId(String customerId);

    /**
     * Find orders by customer ID with pagination
     */
    Page<Order> findByCustomerId(String customerId, Pageable pageable);

    /**
     * Find orders by status
     */
    List<Order> findByStatus(OrderStatus status);

    /**
     * Find orders by status with pagination
     */
    Page<Order> findByStatus(OrderStatus status, Pageable pageable);

    /**
     * Find orders by customer ID and status
     */
    List<Order> findByCustomerIdAndStatus(String customerId, OrderStatus status);

    /**
     * Get orders count by customer
     */
    @Query("SELECT COUNT(o) FROM Order o WHERE o.customerId = :customerId")
    Long countByCustomerId(@Param("customerId") String customerId);

    /**
     * Get total order amount by customer
     */
    @Query("SELECT SUM(o.totalAmount) FROM Order o WHERE o.customerId = :customerId")
    Double getTotalSpentByCustomer(@Param("customerId") String customerId);
}
