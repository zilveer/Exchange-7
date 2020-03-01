import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { OrdersService, OrderResponseModel } from 'src/ApiService'

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent {
  public orders: OrderResponseModel[]
  constructor(private ordersService: OrdersService) {
    ordersService.ordersGet().subscribe(result => {
      this.orders = result
    }, error => console.error(error));
  }
}

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}
