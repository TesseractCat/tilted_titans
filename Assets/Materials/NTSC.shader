Shader "Unlit/NTSC"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ResMult ("Resolution Multiplier", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _ResMult;

            static const float2 iResolution = _MainTex_TexelSize.zw * _ResMult;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // https://www.shadertoy.com/view/llyGzR

            // Modulator
            #define F_COL (1.0 / 4.0)

            static const float pi = 3.14159;
            static const float tau = 6.28319;

            static const float3x3 rgb2yiq = {0.299, 0.587, 0.114,
                                             0.596,-0.274,-0.322,
                                             0.211,-0.523, 0.312};

            //Complex oscillator, Fo = Oscillator freq., Fs = Sample freq., n = Sample index
            float2 Oscillator(float Fo, float Fs, float n)
            {
                float phase = (tau*Fo*floor(n))/Fs;
                return float2(cos(phase),sin(phase));
            }

            float4 modulate( float2 uv )
            {
                float2 fragCoord = uv * iResolution;

                float Fs = iResolution.x;
                float Fcol = Fs * F_COL;
                float n = floor(fragCoord.x);
                
                float3 cRGB = tex2D(_MainTex, fragCoord / iResolution.xy).rgb;
                float3 cYIQ = mul(rgb2yiq, cRGB);
                
                float2 cOsc = Oscillator(Fcol, Fs, n);
                
                float sig = cYIQ.x + dot(cOsc, cYIQ.yz);

                return float4(sig,0,0,0);
            }

            //Demodulator
            #define F_COL (1.0 / 4.0)
            #define F_LUMA_LP (1.0 / 6.0)
            #define F_COL_BW (1.0 / 50.0)

            #define FIR_SIZE 29

            //Complex multiply
            float2 cmul(float2 a, float2 b)
            {
                return float2((a.x * b.x) - (a.y * b.y), (a.x * b.y) + (a.y * b.x));
            }

            float sinc(float x)
            {
                return (x == 0.0) ? 1.0 : sin(x*pi)/(x*pi);   
            }

            //https://en.wikipedia.org/wiki/Window_function
            float WindowBlackman(float a, int N, int i)
            {
                float a0 = (1.0 - a) / 2.0;
                float a1 = 0.5;
                float a2 = a / 2.0;
                
                float wnd = a0;
                wnd -= a1 * cos(2.0 * pi * (float(i) / float(N - 1)));
                wnd += a2 * cos(4.0 * pi * (float(i) / float(N - 1)));
                
                return wnd;
            }

            //FIR lowpass filter 
            //Fc = Cutoff freq., Fs = Sample freq., N = # of taps, i = Tap index
            float Lowpass(float Fc, float Fs, int N, int i)
            {    
                float wc = (Fc/Fs);
                
                float wnd = WindowBlackman(0.16, N, i);
                
                return 2.0*wc * wnd * sinc(2.0*wc * float(i - N/2));
            }

            //FIR bandpass filter 
            //Fa/Fb = Low/High cutoff freq., Fs = Sample freq., N = # of taps, i = Tap index
            float Bandpass(float Fa, float Fb, float Fs, int N, int i)
            {    
                float wa = (Fa/Fs);
                float wb = (Fb/Fs);
                
                float wnd = WindowBlackman(0.16, N, i);
                
                return 2.0*(wb-wa) * wnd * (sinc(2.0*wb * float(i - N/2)) - sinc(2.0*wa * float(i - N/2)));
            }

            float4 demodulate( float2 uv )
            {
                float2 fragCoord = uv * iResolution;

                float Fs = iResolution.x;
                float Fcol = Fs * F_COL;
                float Fcolbw = Fs * F_COL_BW;
                float Flumlp = Fs * F_LUMA_LP;
                float n = floor(fragCoord.x);
                
                float y_sig = 0.0;    
                float iq_sig = 0.0;
                
                float2 cOsc = Oscillator(Fcol, Fs, n);
                
                n += float(FIR_SIZE)/2.0;
                
                //Separate luma(Y) & chroma(IQ) signals
                for(int i = 0;i < FIR_SIZE;i++)
                {
                    int tpidx = FIR_SIZE - i - 1;
                    float lp = Lowpass(Flumlp, Fs, FIR_SIZE, tpidx);
                    float bp = Bandpass(Fcol - Fcolbw, Fcol + Fcolbw, Fs, FIR_SIZE, tpidx);
                    
                    // y_sig += sample2D(iResolution.xy, float2(n - float(i), fragCoord.y)).r * lp;
                    // iq_sig += sample2D(iResolution.xy, float2(n - float(i), fragCoord.y)).r * bp;
                    y_sig += modulate(float2(n - float(i), fragCoord.y)/iResolution.xy).r * lp;
                    iq_sig += modulate(float2(n - float(i), fragCoord.y)/iResolution.xy).r * bp;
                }
                
                //Shift IQ signal down from Fcol to DC 
                float2 iq_sig_mix = cmul(float2(iq_sig, 0), cOsc);
                
                return float4(y_sig, iq_sig_mix, 0);
            }

            // Result
            #define HUE 0.0
            #define SATURATION 35.0
            #define BRIGHTNESS 1.0
            #define FIR_SIZE 29

            static const float3x3 yiq2rgb = {1.000, 0.956, 0.621,
                                            1.000,-0.272,-0.647,
                                            1.000,-1.106, 1.703};

            //Angle -> 2D rotation matrix 
            float2x2 rotate(float a)
            {
                return float2x2( cos(a), -sin(a),
                                 sin(a), cos(a));
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                float2 fragCoord = IN.uv * iResolution;

                float Fs = iResolution.x;
                float Fcol = Fs * F_COL;
                float Flumlp = Fs * F_LUMA_LP;
                float n = floor(fragCoord.x);
                
                float luma = demodulate(fragCoord / iResolution.xy).r;
                float2 chroma = float2(0,0);
                
                //Filtering out unwanted high freqency content from the chroma(IQ) signal.
                for(int i = 0;i < FIR_SIZE;i++)
                {
                    int tpidx = FIR_SIZE - i - 1;
                    float lp = Lowpass(Flumlp, Fs, FIR_SIZE, tpidx);
                    chroma += demodulate((fragCoord - float2(i - FIR_SIZE / 2, 0))/iResolution.xy).yz * lp;
                }
                
                chroma = mul(rotate(tau * HUE), chroma);
                
                // float3 color = mul(yiq2rgb, float3(BRIGHTNESS * luma, chroma * SATURATION));
                float3 color = mul(yiq2rgb, float3(BRIGHTNESS * luma, SATURATION * chroma));

                return float4(color, 1);
                //return demodulate(IN.uv);
                // return float4(float3(BRIGHTNESS * luma, chroma * SATURATION), 1);
                // return float4(40.0*chroma+0.5, 0, 1);
                // return luma;
            }
            ENDCG
        }
    }
}
