using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Globalization;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Threading;
using System.Text.RegularExpressions;

namespace ChaosExpert
{
    abstract class NeuralNetwork
    {                
    }

    abstract class Neuron
    {
        protected float[] weights;        
        protected float outputSignal;
        protected float bias;
        public abstract void setRandomWeights();        
        public float OutputSignal
        {
            get
            {
                return this.outputSignal;
            }
        }

        public float[] Weights
        {
            get
            {
                return weights;
            }
            set
            {
                weights = value;
            }
        }
    }

    class SimpleNeuron : Neuron
    {
        private string functionActivation = "tanh";
        //private float inductionField;
        public float gradient;
        private float momentum = 0.65F;
        private float speedLerning = 0.0045F;
        private float[] deltaWeights;
        private float[] inputs;

        public SimpleNeuron(int numWeights, float bias, string functionActivation)
        {
            this.weights = new float[numWeights];
            this.deltaWeights = new float[numWeights];
            this.bias = bias;
            //this.inductionField = bias;
            this.functionActivation = functionActivation;
            setRandomWeights();
        }

        public override void setRandomWeights()
        {
            Thread.Sleep(5);
            Random rand = new Random(unchecked((int)DateTime.Now.Ticks));
            bool isPlus = true;
            float randVal;
            for (int i = 0; i < weights.Length; i++)
            {
                randVal = (float)(rand.NextDouble() * 0.9F);
                weights[i] = (isPlus) ? randVal : randVal * -1F;
                isPlus = !isPlus;
                deltaWeights[i] = (float)(rand.NextDouble() * 0.9F) * 2F - 1F;
            }
        }

        private float getInductionField()
        {                        
            float inductionField = bias;
            for (int i = 0; i < inputs.Length; i++)
            {
                inductionField += inputs[i] * weights[i];
            }
            return inductionField;
        }

        public void computeOutputSignal(float[] inputSignals)
        {
            inputs = inputSignals;
            float inductionField = getInductionField();
            
            switch (functionActivation)
            {
                case "tanh":
                    float c = 2F / 3F;
                    float b = c * inductionField;
                    outputSignal = (1.7159F * (float)Math.Tanh(b));
                    break;
                default :
                    c = 2F / 3F;
                    b = c * inductionField;
                    outputSignal = (1.7159F * (float)Math.Tanh(b));
                    break;
            }
        }

        public float computeDerivative()
        {
            float inductionField = getInductionField();
            float c = 2F / 3F;
            float a = c / 1.7159F;
            float a1 = (1.7159F - inductionField);
            float a2 = (1.7159F + inductionField);
            switch (functionActivation)
            {
                case "tanh":
                    return (a * a1 * a2);
                default:
                    return (a * a1 * a2);
            }
        }

        public void modifyWeights()
        {
            for (int i = 0; i < weights.Length; i++)
            {                
                float deltaWeightCurrent = speedLerning * (momentum * deltaWeights[i] + (1 - momentum) * gradient * inputs[i]);
                weights[i] = weights[i] - deltaWeightCurrent;
                deltaWeights[i] = deltaWeightCurrent;
            }
            speedLerning *= 0.999999999F;
        }

        public float getWeightedGradient(int weightIndex)
        {
            return gradient * weights[weightIndex];
        }
    }

    class MultilayerPerseptron : NeuralNetwork
    {
        public float upperBound = 0.99999F;
        public float lowerBound = -0.99999F;
        private int numInputSignals;        
        private int numLayers;
        private int[] numNeuronsInLayer;
        private SimpleNeuron[][] NeuronsLayer;
        private float[,] signal;
        private float[,] desiredSignal;
        private float[,] testSignal;
        private float[,] testDesiredSignal;
        private int delayInput;

        public MultilayerPerseptron(int numInputSignals, int numLayers, int[] numNeuronsInLayer, string[][] functionsActivation, float[,] signal, float[,] desiredSignal, int delayInput)
        {
            this.delayInput = delayInput - 1;
            this.numLayers = numLayers;
            this.numNeuronsInLayer = numNeuronsInLayer;
            this.numInputSignals = numInputSignals + this.delayInput;
            //this.numOutputSignals = numOutputSignals;
            int numInputs = this.numInputSignals;
            NeuronsLayer = new SimpleNeuron[numLayers][];
            for (int i = 0; i < numLayers; i++)
            {
                NeuronsLayer[i] = new SimpleNeuron[numNeuronsInLayer[i]];
                for (int j = 0; j < numNeuronsInLayer[i]; j++)
                {
                    NeuronsLayer[i][j] = new SimpleNeuron(numInputs, 0.5F, functionsActivation[i][j]);
                }
                numInputs = numNeuronsInLayer[i];
            } 
            this.signal = signal;
            this.desiredSignal = desiredSignal;
        }

        public float[][][] Weights
        {
            get
            {
                float[][][] weights = new float[numLayers][][];
                for (int i = 0; i < numLayers; i++)
                {
                    weights[i] = new float[numNeuronsInLayer[i]][];
                    for (int j = 0; j < numNeuronsInLayer[i]; j++)
                    {
                        weights[i][j] = NeuronsLayer[i][j].Weights;
                    }
                }
                return weights;
            }

            set
            {
                float[][][] weights = value;
                for (int i = 0; i < numLayers; i++)
                {                    
                    for (int j = 0; j < numNeuronsInLayer[i]; j++)
                    {
                        NeuronsLayer[i][j].Weights = weights[i][j];
                    }
                }
            }
        }

        private float[] _computeLayer(int layerIndex, float[] inputSignals)
        {
            float[] nextInputSignals = new float[numNeuronsInLayer[layerIndex]];
            for (int i = 0; i < numNeuronsInLayer[layerIndex]; i++)
            {
                NeuronsLayer[layerIndex][i].computeOutputSignal(inputSignals);
                nextInputSignals[i] = NeuronsLayer[layerIndex][i].OutputSignal;
            }            
            return nextInputSignals;
        }


        private float[] _computeNet(float[] inputSignals)
        {
            
            for (int i = 0; i < numLayers; i++)
            {
                inputSignals = _computeLayer(i, inputSignals);                
            }
            return inputSignals;
        }

        private void _modifyNet(float[] outputSignals, float[] desiredSignals, float[] errors)
        {            
            int lastLayerIndex = numLayers - 1;
            //Модифицируем веса выходного слоя
            for (int i = 0; i < numNeuronsInLayer[lastLayerIndex]; i++)
            {
                float derivative = NeuronsLayer[lastLayerIndex][i].computeDerivative();
                NeuronsLayer[lastLayerIndex][i].gradient = errors[i] * derivative;
                NeuronsLayer[lastLayerIndex][i].modifyWeights();                
            }
            //Модифицируем веса остальных слоев
            for (int j = lastLayerIndex - 1; j >= 0; j--)
            {
                for (int i = 0; i < numNeuronsInLayer[j]; i++)
                {
                    float derivative = NeuronsLayer[j][i].computeDerivative();
                    float gradientSum = 0;
                    for (int k = 0; k < numNeuronsInLayer[j+1]; k++)
                    {
                        gradientSum += NeuronsLayer[j + 1][k].getWeightedGradient(i);
                    }
                    NeuronsLayer[j][i].gradient = derivative * gradientSum;
                    NeuronsLayer[j][i].modifyWeights();
                }
            }
        }

        private float _trainEpoch(int numInputSignals, int numDesiredSignals, int trainLength)
        {
            float[] inputSignals = new float[numInputSignals];
            float[] desiredSignals = new float[numDesiredSignals];
            float[] errors = new float[numDesiredSignals];
            float[] errorsGlob = new float[numDesiredSignals];
            float[] outputSignals;
            float[] err = new float[numDesiredSignals];            
            for (int i = delayInput; i < trainLength; i++)
            {
                for (int j = 0; j < inputSignals.Length; j++)
                {
                    inputSignals[j] = signal[i - j, 0];
                }
                for (int j = 0; j < desiredSignals.Length; j++)
                {                    
                    desiredSignals[j] = desiredSignal[i, j];
                }
                outputSignals = _computeNet(inputSignals);
                for (int k = 0; k < errors.Length; k++)
                {
                    err[k] = outputSignals[k] - desiredSignals[k];
                    errors[k] = err[k] * err[k];
                    errorsGlob[k] += errors[k];
                }
                _modifyNet(outputSignals, desiredSignals, err);
            }
            float MSE = 0;
            for (int k = 0; k < errorsGlob.Length; k++)
            {
                MSE += errorsGlob[k];
            }
            MSE /= signal.GetLength(0);
            return MSE;
        }

        private float _getProcentDifference(float[] realSignal, float[] predictSignal)
        {
            float procent = 0;
            for (int i = 0; i < realSignal.Length; i++)
            {
                procent += Math.Abs((realSignal[i] - predictSignal[i]) / realSignal[i]) * 100;
            }
            procent /= realSignal.Length;
            return procent;
        }

        public MlpTrainResult train(int numEpochs, int trainLength, float[] amplituda, float[] offset)
        {                        
            int epochCount = 0;
            float[] errors = new float[numEpochs];
            int numInputSignals = signal.GetLength(1) + delayInput;
            int numDesiredSignals = desiredSignal.GetLength(1);
            trainLength = (trainLength == 0) ? signal.GetLength(0) : trainLength;            
            float[][][] bestWeights = this.Weights;

            float[] checkSignal = new float[numInputSignals];
            float[] checkSignal2 = new float[numInputSignals];

            float[] checkPredict = new float[numDesiredSignals];
            float[] realSignal = new float[numDesiredSignals];
            float[] realSignal2 = new float[numDesiredSignals];

            float[] checkPredict2 = new float[numDesiredSignals];
            float[] checkPredict1B = new float[numInputSignals];
            float[] checkPredict2B = new float[numInputSignals];

            for (int j = 0; j < numInputSignals; j++)
            {
                checkSignal[j] = signal[trainLength + j - delayInput, 0];
                checkSignal2[j] = signal[trainLength + j - delayInput + 1, 0];
            }
            realSignal[0] = desiredSignal[trainLength+1, 0];
            realSignal2[0] = desiredSignal[trainLength + 2, 0];
            realSignal[0] = (realSignal[0] - offset[0]) / amplituda[0];
            realSignal2[0] = (realSignal2[0] - offset[0]) / amplituda[0];

            float minErr = 1000;
            float procentErr, procentErr1, procentErr2;
            float procentErrB1 = 0;
            float procentErrB2 = 0;
            float y1, y2, k, b;
            k = 0;
            b = 0;
            int numberEpochCount = 0;
            MlpTrainResult res = new MlpTrainResult();
            while (epochCount < numEpochs)
            {                
                errors[epochCount] = _trainEpoch(numInputSignals, numDesiredSignals, trainLength);                
                checkPredict = _computeNet(checkSignal);
                checkSignal2[numInputSignals - 1] = checkPredict[0];
                checkPredict2 = _computeNet(checkSignal2);
                checkPredict[0] = (checkPredict[0] - offset[0]) / amplituda[0];                
                procentErr1 = _getProcentDifference(realSignal, checkPredict);                                
                checkPredict2[0] = (checkPredict2[0] - offset[0]) / amplituda[0];                
                procentErr2 = _getProcentDifference(realSignal2, checkPredict2);
                procentErr = (procentErr1 + procentErr2)/2;
                if (procentErr1 < minErr)
                {
                    minErr = procentErr1;
                    bestWeights = this.Weights;
                    numberEpochCount = epochCount;
                    procentErrB1 = procentErr1;
                    procentErrB2 = procentErr2;
                    
                    res.checkSignal = checkSignal;
                    res.realSignal = realSignal;
                    res.predictSignal = checkPredict;
                    res.realSignal2 = realSignal2;
                    res.predictSignal2 = checkPredict2;
                }
                
                /*
                if (errors[epochCount] < minError)
                {
                    bestWeights = this.Weights;
                    numberBestEpoch = epochCount;
                    minError = errors[epochCount];
                }
                else
                {
                    //numStop++;
                }
                 */ 
                //if (numStop > 10) break;
                //DataProcess.shuffleArrays(ref signal, ref desiredSignal, trainLength);
                epochCount++;
            }
            this.Weights = bestWeights;
            
            /*

            for (int i = 0; i < 10; i++)
            {
                _trainEpoch(numInputSignals, numDesiredSignals, trainLength + 2);
            }
            */

            DataProcess.ExportArray(errors, "_mlp_err.csv");
            string report = "";
            report += "\n minErr = " + minErr.ToString();
            report += "\n numberEpochCount = " + numberEpochCount.ToString();
            report += "\n procentErrB1 = " + procentErrB1.ToString();
            report += "\n procentErrB2 = " + procentErrB2.ToString();
            
            res.report = report;            
            return res;
        }

        public float atanh(float x)
        {
            return (float)Math.Log((1 - x) / (1 + x));
        }

        public float[, ,] test(float[,] testSignal, float[,] testDesiredSignal, float[] offset, float[] amplituda)
        {
            this.testSignal = testSignal;
            this.testDesiredSignal = testDesiredSignal;

            int numInputSignals = testSignal.GetLength(1);
            int numDesiredSignals = testDesiredSignal.GetLength(1);
            float[] inputSignals = new float[numInputSignals];
            float[] desiredSignals = new float[numDesiredSignals];
            float[] outputSignals;
            int testLength = testSignal.GetLength(0);
            float[, ,] testOut = new float[testLength, numDesiredSignals, 3];
            for (int i = 0; i < testLength; i++)
            {
                for (int j = 0; j < numInputSignals; j++) inputSignals[j] = testSignal[i, j];
                for (int j = 0; j < numDesiredSignals; j++) desiredSignals[j] = testDesiredSignal[i, j];
                outputSignals = _computeNet(inputSignals);
                for (int k = 0; k < outputSignals.Length; k++)
                {
                    testOut[i, k, 0] = (outputSignals[k] - offset[k]) / amplituda[k];
                    testOut[i, k, 1] = (desiredSignals[k] - offset[k]) / amplituda[k];
                    testOut[i, k, 2] = (testOut[i, k, 1] - testOut[i, k, 0]) / testOut[i, k, 1] * 100;
                }                
            }
            return testOut;
        }

        public string recursivePrediction(int numStepRecursPrediction, float[] amplituda, float[] offset)
        {
            int length = signal.GetLength(0);
            int length2 = signal.GetLength(1);

            float[,] predictSignal = new float[numStepRecursPrediction + delayInput, length2];
            float[,] desiredSignal = new float[numStepRecursPrediction, length2];
            float[,] testSignal = new float[numStepRecursPrediction + delayInput, length2 + delayInput];            
            for (int i = 0; i < numStepRecursPrediction; i++)
            {
                for (int k = 0; k < length2 + delayInput; k++)
                {
                    testSignal[i, k] = signal[i + length - numStepRecursPrediction - delayInput + k, 0];
                }
            }
            MlpTrainResult resTrain = train(1000, length - numStepRecursPrediction, amplituda, offset);

            float[] val = new float[length2 + delayInput];

            for (int k = 0; k < length2 + delayInput; k++)
            {
                predictSignal[k, 0] = testSignal[0, k];
            }

            predictSignal[delayInput, 0] = resTrain.checkSignal[delayInput];
            desiredSignal[0, 0] = resTrain.checkSignal[delayInput];
            predictSignal[delayInput + 1, 0] = resTrain.predictSignal[0] * amplituda[0] + offset[0];
            desiredSignal[1, 0] = resTrain.realSignal[0] * amplituda[0] + offset[0];
            predictSignal[delayInput + 2, 0] = resTrain.predictSignal2[0] * amplituda[0] + offset[0];
            desiredSignal[2, 0] = resTrain.realSignal2[0] * amplituda[0] + offset[0];

            for (int j = 0; j < length2 + delayInput; j++)
            {
                val[j] = testSignal[2, j];
            }
            
            float[] newVal;            
            for (int i = 3; i < numStepRecursPrediction - 1; i++)
            {
                newVal = _computeNet(val);
                for (int j=0;j<length2;j++)
                {
                    predictSignal[i + delayInput, j] = newVal[j];
                    desiredSignal[i, j] = testSignal[i, delayInput];
                }
                for (int j = 0; j < length2 + delayInput; j++)
                {
                    val[j] = predictSignal[i + j, 0];
                }
            }
            float[, ,] testOut = new float[numStepRecursPrediction, length2, 3];
            for (int i = 0; i < numStepRecursPrediction; i++)
            {
                for (int k = 0; k < length2; k++)
                {
                    testOut[i, k, 0] = (predictSignal[i + delayInput, k] - offset[k]) / amplituda[k];
                    testOut[i, k, 1] = (desiredSignal[i, k] - offset[k]) / amplituda[k];
                    testOut[i, k, 2] = (testOut[i, k, 1] - testOut[i, k, 0]) / testOut[i, k, 1] * 100;
                }
            }
            DataProcess.ExportArray3(testOut, "_test.csv");
            return resTrain.report;
        }
    }
}