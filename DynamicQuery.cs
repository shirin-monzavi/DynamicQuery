    private static ExpressionStarter<T> GenerateDynamicConditionForFindingRangeOfIds<T>(IEnumerable<DataBaseTransfer> databaseTransfer,
                                                                                        ExpressionStarter<T> predicate,
                                                                                        DynamicConditionInputDto? dynamicConditionInputDto = null
                                                                                        ) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), ParameterName);

        foreach (var item in databaseTransfer)
        {
            if (dynamicConditionInputDto!.DataType == DataType.Numeber)
            {
                var predicate1 = Expression.Lambda<Func<T, bool>>(Expression.GreaterThanOrEqual(Expression.PropertyOrField(parameter, dynamicConditionInputDto.PropertyName), Expression.Constant(Convert.ToInt32(item.FirstRecord))), parameter);
                var predicate2 = Expression.Lambda<Func<T, bool>>(Expression.LessThanOrEqual(Expression.PropertyOrField(parameter, dynamicConditionInputDto.PropertyName), Expression.Constant(Convert.ToInt32(item.LastRecord))), parameter);

                predicate = predicate.Or(predicate1.And(predicate2));
                continue;
            }

            if (predicate is ExpressionStarter<GetCustomerDto> customer)
            {
                customer = customer.Or(c => string.Compare(c.C_Code, item.FirstRecord) >= 0 &&
                                            string.Compare(c.C_Code, item.LastRecord) <= 0);
                continue;
            }

            if (predicate is ExpressionStarter<GetCusTellDto> getCusTellDto)
            {
                getCusTellDto = getCusTellDto.Or(c => string.Compare(c.CCode, item.FirstRecord) >= 0 &&
                                            string.Compare(c.CCode, item.LastRecord) <= 0);

                continue;
            }

            if (predicate is ExpressionStarter<GetInvoiceDetailDto> invoiceDetail)
            {
                invoiceDetail = invoiceDetail.Or(c => string.Compare(c.Id, item.FirstRecord) >= 0 &&
                                                      string.Compare(c.Id, item.LastRecord) <= 0);
                continue;
            }

            if (predicate is ExpressionStarter<GetInvoiceHeaderDto> invoiceHeader)
            {
                invoiceHeader = invoiceHeader.Or(c => string.Compare(c.Fac_Code, item.FirstRecord) >= 0 &&
                                                      string.Compare(c.Fac_Code, item.LastRecord) <= 0);
                continue;
            }
        }

        return predicate;
    }



    private static void RemoveIsThirdPartyFromData<T>(ExclusionThirdPartyDataInDatabaseTransferDto databaseTransferAndExcludingThirdPartyIds,
                                                      List<T> data,
                                                      DynamicConditionInputDto dynamicConditionInputDto
                                                      ) where T : class
    {
        Func<T, int> numericGetter;
        Func<T, string> stringGetter;
        bool checkDataTypeIdNumber;

        GenerateDelegateForReflection(dynamicConditionInputDto, out numericGetter, out stringGetter, out checkDataTypeIdNumber);

        foreach (var item in databaseTransferAndExcludingThirdPartyIds.RangeId)
        {
            if (checkDataTypeIdNumber)
            {
                data.RemoveAll(x => numericGetter(x) >= Convert.ToInt32(item.Item1) &&
                                    numericGetter(x) <= Convert.ToInt32(item.Item2));
                continue;
            }

            data.RemoveAll(x => string.Compare(stringGetter(x), item.Item1) >= 0 &&
                                string.Compare(stringGetter(x), item.Item2) <= 0);
        }
    }

    private static void GenerateDelegateForReflection<T>(DynamicConditionInputDto dynamicConditionInputDto,
                                                         out Func<T, int> numericGetter,
                                                         out Func<T, string> stringGetter,
                                                         out bool checkDataTypeIdNumber) where T : class
    {
        numericGetter = (x) => InitializeNumericFunc;
        stringGetter = (x) => InitializeTextFunc;

        checkDataTypeIdNumber = dynamicConditionInputDto.DataType == DataType.Numeber;

        if (checkDataTypeIdNumber)
        {
            numericGetter = (Func<T, int>)Delegate.CreateDelegate(typeof(Func<T, int>), null, typeof(T).GetProperty(dynamicConditionInputDto.PropertyName)!
                                                                                                        .GetGetMethod()!);
            return;
        }

        stringGetter = (Func<T, string>)Delegate.CreateDelegate(typeof(Func<T, string>), null, typeof(T).GetProperty(dynamicConditionInputDto.PropertyName)!
                                                                                                         .GetGetMethod()!);
    }
