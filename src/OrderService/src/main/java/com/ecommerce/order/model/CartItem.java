package com.ecommerce.order.model;

import lombok.*;
import java.io.Serializable;
import java.math.BigDecimal;

/**
 * CartItem DTO
 * 
 * Represents an item in the shopping cart.
 * Uses Redis for session storage.
 */
@Data
@NoArgsConstructor
@AllArgsConstructor
@Builder
public class CartItem implements Serializable {

    private static final long serialVersionUID = 1L;

    private String productId;
    private String productName;
    private String productSku;
    private BigDecimal price;
    private Integer quantity;
    private BigDecimal subtotal;

    /**
     * Calculate subtotal for this cart item
     */
    public void calculateSubtotal() {
        this.subtotal = this.price.multiply(BigDecimal.valueOf(this.quantity));
    }
}
