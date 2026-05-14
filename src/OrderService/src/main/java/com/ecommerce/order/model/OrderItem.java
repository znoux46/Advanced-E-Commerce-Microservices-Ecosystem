package com.ecommerce.order.model;

import jakarta.persistence.*;
import lombok.*;
import java.math.BigDecimal;
import java.util.UUID;

/**
 * OrderItem Entity
 * 
 * Represents an item in an order.
 * Links to Product from Product Service.
 */
@Entity
@Table(name = "order_items")
@Data
@NoArgsConstructor
@AllArgsConstructor
@Builder
public class OrderItem {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private String id;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "order_id", nullable = false)
    private Order order;

    @Column(name = "product_id", nullable = false)
    private String productId;

    @Column(name = "product_name", nullable = false)
    private String productName;

    @Column(name = "product_sku")
    private String productSku;

    @Column(name = "quantity", nullable = false)
    private Integer quantity;

    @Column(name = "price", precision = 18, scale = 2, nullable = false)
    private BigDecimal price;

    @Column(name = "subtotal", precision = 18, scale = 2)
    private BigDecimal subtotal;

    @PrePersist
    @PreUpdate
    protected void calculateSubtotal() {
        this.subtotal = this.price.multiply(BigDecimal.valueOf(this.quantity));
    }
}
