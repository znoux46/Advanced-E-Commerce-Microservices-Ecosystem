package com.ecommerce.order.dto;

import jakarta.validation.constraints.*;
import lombok.*;
import java.math.BigDecimal;
import java.util.List;

/**
 * Create Order Request DTO
 */
@Data
@NoArgsConstructor
@AllArgsConstructor
@Builder
public class CreateOrderRequest {

    @NotBlank(message = "Customer ID is required")
    private String customerId;

    @NotBlank(message = "Shipping address is required")
    private String shippingAddress;

    private String billingAddress;

    @NotBlank(message = "Payment method is required")
    private String paymentMethod;

    private String notes;

    @NotEmpty(message = "Order items are required")
    private List<CreateOrderItemRequest> items;
}

/**
 * Create Order Item Request DTO
 */
@Data
@NoArgsConstructor
@AllArgsConstructor
@Builder
class CreateOrderItemRequest {

    @NotBlank(message = "Product ID is required")
    private String productId;

    @NotBlank(message = "Product name is required")
    private String productName;

    private String productSku;

    @NotNull(message = "Quantity is required")
    @Min(value = 1, message = "Quantity must be at least 1")
    private Integer quantity;

    @NotNull(message = "Price is required")
    @Positive(message = "Price must be positive")
    private BigDecimal price;
}
