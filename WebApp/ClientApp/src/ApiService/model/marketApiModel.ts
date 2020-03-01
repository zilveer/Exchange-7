/**
 * Exchange API
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: 0.9
 * 
 *
 * NOTE: This class is auto generated by the swagger code generator program.
 * https://github.com/swagger-api/swagger-codegen.git
 * Do not edit the class manually.
 */


export interface MarketApiModel { 
    id: number;
    tradeCurrencyId: number;
    quoteCurrencyId: number;
    minQuantity: number;
    minRate: number;
    maxRate: number;
    newOrderAllowed: boolean;
    cancelAllowed: boolean;
    maxQuantity: number;
    quantityStepSize: number;
    rateStepSize: number;
    makerFeePercent: number;
    takerFeePercent: number;
}
