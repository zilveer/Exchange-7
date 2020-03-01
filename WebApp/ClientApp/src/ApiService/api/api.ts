export * from './balances.service';
import { BalancesService } from './balances.service';
export * from './currencies.service';
import { CurrenciesService } from './currencies.service';
export * from './markets.service';
import { MarketsService } from './markets.service';
export * from './orders.service';
import { OrdersService } from './orders.service';
export const APIS = [BalancesService, CurrenciesService, MarketsService, OrdersService];
