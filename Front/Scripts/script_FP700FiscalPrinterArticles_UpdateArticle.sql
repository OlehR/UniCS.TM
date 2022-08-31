UPDATE FP700FiscalPrinterArticles 
			SET 
			Price = @Price,
			ProductName = @ProductName
			WHERE Barcode = @Barcode