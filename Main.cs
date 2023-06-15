using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace ArsamCodex
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ~Author~ Sami
            //Pancakeswap solidity conract to read and undrestand mechanism that they use sinds there is no new dev and no active api and no reply in Guthub
            string pancakeSwapContract = "0x10ED43C718714eb63d5aA57B78B54704E256024E".ToLower();
            //Here token address you want see Enter Token address here
            string tokenAddress = "0xa49e44976c236beb51a1f818d49b9b9759ed97b1";

            Web3 web3 = new Web3("https://bsc-dataseed1.binance.org");

            decimal bnbPrice = await CalcBNBPrice(web3, pancakeSwapContract);
           

            decimal tokensToSell = 1;
            decimal priceInBnb = await CalcSell(web3, tokensToSell, tokenAddress, pancakeSwapContract);
            Console.WriteLine("SHIT_TOKEN VALUE IN BNB: " + priceInBnb + " | Just convert it to USD");

            decimal tokenValueInUsd = priceInBnb * bnbPrice;
            Console.WriteLine($"SHIT_TOKEN VALUE IN USD: {tokenValueInUsd}");
        }

        private static async Task<decimal> CalcSell(Web3 web3, decimal tokensToSell, string tokenAddress, string pancakeSwapContract)
        {
            string bnbTokenAddress = "0xbb4CdB9CBd36B01bD1cBaEBF2De08d9173bc095c";
            var tokenDecimals = await GetDecimals(web3, tokenAddress);

            decimal tokensToSellWithDecimals = SetDecimals(tokensToSell, tokenDecimals);

            try
            {
                var router = web3.Eth.GetContractQueryHandler<GetAmountsOutFunction>();
                var result = await router.QueryDeserializingToObjectAsync<AmountsOutDTO>(
                    new GetAmountsOutFunction
                    {
                        AmountIn = BigInteger.Parse(tokensToSellWithDecimals.ToString()),
                        Path = new List<string> { tokenAddress, bnbTokenAddress }
                    },
                    pancakeSwapContract);

                // Parse the result to extract the amountOut value
                decimal amountOutValue = Web3.Convert.FromWei(result.Amounts[1]);

                return amountOutValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred during CalcSell:");
                Console.WriteLine(ex.Message);
                return 0;
            }
        }




        private static async Task<decimal> CalcBNBPrice(Web3 web3, string pancakeSwapContract)
        {
            string bnbTokenAddress = "0xbb4CdB9CBd36B01bD1cBaEBF2De08d9173bc095c";
            string usdTokenAddress = "0x55d398326f99059fF775485246999027B3197955";
            decimal bnbToSell = 1;

            try
            {
                var router = web3.Eth.GetContractQueryHandler<GetAmountsOutFunction>();
                var result = await router.QueryDeserializingToObjectAsync<AmountsOutDTO>(
                    new GetAmountsOutFunction
                    {
                        AmountIn = BigInteger.Parse(bnbToSell.ToString()),
                        Path = new List<string> { bnbTokenAddress, usdTokenAddress }
                    },
                    pancakeSwapContract);

                // Parse the result to extract the amountOut value
                decimal amountOutValue = Web3.Convert.FromWei(result.Amounts[1]);

                return amountOutValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred during CalcBNBPrice:");
                Console.WriteLine(ex.Message);
                return 0;
            }
        }


        private static async Task<int> GetDecimals(Web3 web3, string tokenAddress)
        {
            var tokenRouter = web3.Eth.GetContractQueryHandler<DecimalsFunction>();
            var decimalsResult = await tokenRouter.QueryAsync<int>(tokenAddress);
            return decimalsResult;
        }

        private static decimal SetDecimals(decimal number, int decimals)
        {
            string numberString = number.ToString();
            string[] parts = numberString.Split('.');

            string numberAbs = parts[0];
            string numberDecimals = parts.Length > 1 ? parts[1] : "";

            while (numberDecimals.Length < decimals)
            {
                numberDecimals += "0";
            }

            string resultString = numberAbs + numberDecimals;
            return decimal.Parse(resultString);
        }
    }

    [Function("getAmountsOut", "uint256[]")]
    public class GetAmountsOutFunction : FunctionMessage
    {
        [Parameter("uint256", "amountIn", 1)]
        public BigInteger AmountIn { get; set; }

        [Parameter("address[]", "path", 2)]
        public List<string> Path { get; set; }
    }

    [Function("decimals", "uint256")]
    public class DecimalsFunction : FunctionMessage
    {
    }
    [FunctionOutput]
    public class AmountsOutDTO : IFunctionOutputDTO
    {
        [Parameter("uint256[]", "amounts", 1)]
        public List<BigInteger> Amounts { get; set; }
    }
}
