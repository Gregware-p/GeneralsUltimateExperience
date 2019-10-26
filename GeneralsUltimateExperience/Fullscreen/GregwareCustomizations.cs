using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Gma.System.MouseKeyHook;

namespace GeneralsUltimateExperience.Fullscreen
{
    public class GregwareCustomizations
    {
        #region External functions stuff
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        public const UInt32 SWP_NOSIZE = 0x0001;
        public const UInt32 SWP_NOMOVE = 0x0002;
        public const UInt32 SWP_SHOWWINDOW = 0x0040;

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct INPUT
        {
            [FieldOffset(0)]
            public int type;
            [FieldOffset(4)]
            public MOUSEINPUT mi;
            [FieldOffset(4)]
            public KEYBDINPUT ki;
            [FieldOffset(4)]
            public HARDWAREINPUT hi;
        }

        struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        static extern UInt32 SendInput(UInt32 nInputs, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] INPUT[] pInputs, Int32 cbSize);
        #endregion

        #region constants
        private const int EDGE_SCROLL_PX = 5;
        private enum EdgeZone
        {
            NONE,
            L, R, B, T,
            LT, LB, RT, RB
        }
        private enum KEYS_DX
        {
            ALT = 0x38,
            LEFT = 0xCB,
            RIGHT = 0xCD,
            UP = 0xC8,
            BOTTOM = 0xD0
        }
        #endregion

        #region variables
        private bool _isFullscreenGregware;
        private bool _isScrollGregware;
        private bool _isGentool;
        private int _width;
        private int _height;
        private DisplaySettings _originalDisplaySettings;
        private bool _enabled = false;
        private bool _resolutionChanged = false;
        private bool _activateHook = false;
        private bool _isDownAlt = false, _isDownLeft = false, _isDownRight = false, _isDownTop = false, _isDownBottom = false;
        private bool _isDownW = false, _isDownA = false, _isDownS = false, _isDownD = false;
        private EdgeZone _edgeZoneMouse = EdgeZone.NONE;
        private EdgeZone _edgeZoneKeyboard = EdgeZone.NONE;
        private TaskScheduler _uiScheduler;
        private IKeyboardMouseEvents _globalHook;
        #endregion

        #region constructor
        public GregwareCustomizations(bool isFullscreenGregware, bool isScrollGregware, bool isGentool, int width, int height, TaskScheduler uiScheduler)
        {
            _isFullscreenGregware = isFullscreenGregware;
            _isScrollGregware = isScrollGregware;
            _isGentool = isGentool;
            _width = width;
            _height = height;
            _uiScheduler = uiScheduler;
            _isGentool = isGentool;
            _originalDisplaySettings = DisplayManager.GetCurrentSettings();
        }
        #endregion

        #region Methods
        public void Activer()
        {
            // Initialisation
            _enabled = true;

            // Trouver le process
            Process process = Process.GetProcessesByName("game.dat").FirstOrDefault();
            if (process == null) throw new Exception("Problème d'exécution du jeu, si cette erreur persiste essayer de désactiver les options Gregware (fullscreen et scroll)");

            if (_isFullscreenGregware)
            {
                // Changer resolution
                if (_originalDisplaySettings.Width != _width || _originalDisplaySettings.Height != _height)
                {
                    DisplaySettings newDs = new DisplaySettings
                    {
                        BitCount = _originalDisplaySettings.BitCount,
                        Frequency = _originalDisplaySettings.Frequency,
                        Index = _originalDisplaySettings.Index,
                        Orientation = _originalDisplaySettings.Orientation,
                        Width = _width,
                        Height = _height
                    };
                    DisplayManager.SetDisplaySettings(newDs);
                    _resolutionChanged = true;
                }

                // Attendre
                if (_width == 800)
                {
                    // si width de 800 on ne peut pas différencier splash screen du jeu, on attend 10 secondes bêtement...
                    Thread.Sleep(10000);
                }
                else
                {
                    // sinon on peut différencier, on attend juste ce qu'il faut - 10 sec max
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    Rect rect = new Rect();
                    GetWindowRect(process.MainWindowHandle, ref rect);
                    while (rect.Right - rect.Left != _width && sw.Elapsed.Milliseconds < 10000)
                    {
                        Thread.Sleep(100);
                        GetWindowRect(process.MainWindowHandle, ref rect);
                    }
                }

                // No top most
                SetWindowPos(process.MainWindowHandle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

                // Replacer la fenêtre
                MoveWindow(process.MainWindowHandle, 0, 0, _width, _height, true);
            }

            // Focus
            Task.Factory.StartNew(() =>
            {
                while (_enabled)
                {
                    Thread.Sleep(200);
                    _activateHook = GetForegroundWindow() == process.MainWindowHandle;
                }
            });

            // Hook
            Task.Factory.StartNew(() =>
            {
                _globalHook = Hook.GlobalEvents();
                if (_isFullscreenGregware && !_isGentool) _globalHook.MouseMoveExt += GlobalHookMouseMoveExt;
                if (_isScrollGregware)
                {
                    _globalHook.KeyDown += _globalHook_KeyDown;
                    _globalHook.KeyUp += _globalHook_KeyUp;
                }
            }, CancellationToken.None, TaskCreationOptions.None, _uiScheduler);

            // Scroll
            Task.Factory.StartNew(() =>
            {
                while (_enabled)
                {
                    // Simuler en envoyant des pressions de touche du clavier
                    Thread.Sleep(50);

                    // Cumuler les actions de souris et clavier.
                    EdgeZone edgeZoneCumul;
                    if (!_activateHook)
                    {
                        edgeZoneCumul = EdgeZone.NONE;
                    }
                    else
                    {
                        if (_edgeZoneKeyboard == _edgeZoneMouse) edgeZoneCumul = _edgeZoneKeyboard; // Tout va bien, on est d'accord
                        else if (_edgeZoneKeyboard == EdgeZone.NONE) edgeZoneCumul = _edgeZoneMouse; // Que la souris en scroll
                        else if (_edgeZoneMouse == EdgeZone.NONE) edgeZoneCumul = _edgeZoneKeyboard; // Que le clavier en scroll
                        else edgeZoneCumul = EdgeZone.NONE; // Il y a conflit, on ne fait rien
                    }

                    // Envoyer les keys
                    switch (edgeZoneCumul)
                    {
                        case EdgeZone.NONE:
                            // Toutes les touches UP
                            if (_isDownAlt) SendKey(KEYS_DX.ALT, false);
                            if (_isDownLeft) SendKey(KEYS_DX.LEFT, false);
                            if (_isDownRight) SendKey(KEYS_DX.RIGHT, false);
                            if (_isDownBottom) SendKey(KEYS_DX.BOTTOM, false);
                            if (_isDownTop) SendKey(KEYS_DX.UP, false);
                            break;

                        case EdgeZone.L:
                            // Left et Alt DOWN
                            if (!_isDownAlt) SendKey(KEYS_DX.ALT, true);
                            if (!_isDownLeft) SendKey(KEYS_DX.LEFT, true);

                            // les autre UP
                            if (_isDownRight) SendKey(KEYS_DX.RIGHT, false);
                            if (_isDownBottom) SendKey(KEYS_DX.BOTTOM, false);
                            if (_isDownTop) SendKey(KEYS_DX.UP, false);
                            break;

                        case EdgeZone.R:
                            // Right et Alt DOWN
                            if (!_isDownAlt) SendKey(KEYS_DX.ALT, true);
                            if (!_isDownRight) SendKey(KEYS_DX.RIGHT, true);

                            // les autre UP
                            if (_isDownLeft) SendKey(KEYS_DX.LEFT, false);
                            if (_isDownBottom) SendKey(KEYS_DX.BOTTOM, false);
                            if (_isDownTop) SendKey(KEYS_DX.UP, false);
                            break;

                        case EdgeZone.T:
                            // Top et Alt DOWN
                            if (!_isDownAlt) SendKey(KEYS_DX.ALT, true);
                            if (!_isDownTop) SendKey(KEYS_DX.UP, true);

                            // les autre UP
                            if (_isDownLeft) SendKey(KEYS_DX.LEFT, false);
                            if (_isDownBottom) SendKey(KEYS_DX.BOTTOM, false);
                            if (_isDownRight) SendKey(KEYS_DX.RIGHT, false);
                            break;

                        case EdgeZone.B:
                            // Bottom et Alt DOWN
                            if (!_isDownAlt) SendKey(KEYS_DX.ALT, true);
                            if (!_isDownBottom) SendKey(KEYS_DX.BOTTOM, true);

                            // les autre UP
                            if (_isDownLeft) SendKey(KEYS_DX.LEFT, false);
                            if (_isDownTop) SendKey(KEYS_DX.UP, false);
                            if (_isDownRight) SendKey(KEYS_DX.RIGHT, false);
                            break;

                        case EdgeZone.LT:
                            // Left, Top et Alt DOWN
                            if (!_isDownAlt) SendKey(KEYS_DX.ALT, true);
                            if (!_isDownTop) SendKey(KEYS_DX.UP, true);
                            if (!_isDownLeft) SendKey(KEYS_DX.LEFT, true);

                            // les autre UP
                            if (_isDownRight) SendKey(KEYS_DX.RIGHT, false);
                            if (_isDownBottom) SendKey(KEYS_DX.BOTTOM, false);
                            break;

                        case EdgeZone.LB:
                            // Left, Bottom et Alt DOWN
                            if (!_isDownAlt) SendKey(KEYS_DX.ALT, true);
                            if (!_isDownBottom) SendKey(KEYS_DX.BOTTOM, true);
                            if (!_isDownLeft) SendKey(KEYS_DX.LEFT, true);

                            // les autre UP
                            if (_isDownRight) SendKey(KEYS_DX.RIGHT, false);
                            if (_isDownTop) SendKey(KEYS_DX.UP, false);
                            break;

                        case EdgeZone.RT:
                            // Right, Top et Alt DOWN
                            if (!_isDownAlt) SendKey(KEYS_DX.ALT, true);
                            if (!_isDownTop) SendKey(KEYS_DX.UP, true);
                            if (!_isDownRight) SendKey(KEYS_DX.RIGHT, true);

                            // les autre UP
                            if (_isDownLeft) SendKey(KEYS_DX.LEFT, false);
                            if (_isDownBottom) SendKey(KEYS_DX.BOTTOM, false);
                            break;

                        case EdgeZone.RB:
                            // Right, Bottom et Alt DOWN
                            if (!_isDownAlt) SendKey(KEYS_DX.ALT, true);
                            if (!_isDownBottom) SendKey(KEYS_DX.BOTTOM, true);
                            if (!_isDownRight) SendKey(KEYS_DX.RIGHT, true);

                            // les autre UP
                            if (_isDownLeft) SendKey(KEYS_DX.LEFT, false);
                            if (_isDownTop) SendKey(KEYS_DX.UP, false);
                            break;
                    }
                }
            });
        }

        public void Desactiver()
        {
            if (_resolutionChanged) DisplayManager.SetDisplaySettings(_originalDisplaySettings);
            Task.Factory.StartNew(() =>
            {
                if (_isFullscreenGregware && !_isGentool) _globalHook.MouseMoveExt -= GlobalHookMouseMoveExt;
                if (_isScrollGregware)
                {
                    _globalHook.KeyDown -= _globalHook_KeyDown;
                    _globalHook.KeyUp -= _globalHook_KeyUp;
                }
                _globalHook.Dispose();
            }, CancellationToken.None, TaskCreationOptions.None, _uiScheduler);
            _enabled = false;
        }
        #endregion

        #region Helpers
        private void GlobalHookMouseMoveExt(object sender, MouseEventExtArgs e)
        {
            if (!_activateHook) return;

            // Remettre dans la fenêtre si dépasse
            int newX = e.X;
            int newY = e.Y;
            if (e.X >= _width - 1) newX = _width - 1;
            else if (e.X < 1) newX = 1;
            if (e.Y >= _height - 1) newY = _height - 1;
            else if (e.Y < 1) newY = 1;
            if (newX != e.X || newY != e.Y)
            {
                SetCursorPos(newX, newY);
                e.Handled = true;
            }

            // Détecter si dans zone de mouvement
            bool left, right, top, bottom, isZone;
            left = newX <= EDGE_SCROLL_PX;
            right = newX >= _width - EDGE_SCROLL_PX;
            top = newY <= EDGE_SCROLL_PX;
            bottom = newY >= _height - EDGE_SCROLL_PX;
            isZone = left || right || top || bottom;

            if (!isZone) _edgeZoneMouse = EdgeZone.NONE;
            else if (left && top) _edgeZoneMouse = EdgeZone.LT;
            else if (left && bottom) _edgeZoneMouse = EdgeZone.LB;
            else if (right && top) _edgeZoneMouse = EdgeZone.RT;
            else if (right && bottom) _edgeZoneMouse = EdgeZone.RB;
            else if (left) _edgeZoneMouse = EdgeZone.L;
            else if (right) _edgeZoneMouse = EdgeZone.R;
            else if (top) _edgeZoneMouse = EdgeZone.T;
            else if (bottom) _edgeZoneMouse = EdgeZone.B;
        }

        private void _globalHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            TraiterKey(true, e);
        }

        private void _globalHook_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            TraiterKey(false, e);
        }

        private void TraiterKey(bool isDown, System.Windows.Forms.KeyEventArgs e)
        {
            // Validité
            if (!_activateHook) return;
            if (e.KeyCode != System.Windows.Forms.Keys.W &&
                e.KeyCode != System.Windows.Forms.Keys.A &&
                e.KeyCode != System.Windows.Forms.Keys.S &&
                e.KeyCode != System.Windows.Forms.Keys.D) return;

            // Assigner les variables
            if (e.KeyCode == System.Windows.Forms.Keys.W) _isDownW = isDown;
            if (e.KeyCode == System.Windows.Forms.Keys.A) _isDownA = isDown;
            if (e.KeyCode == System.Windows.Forms.Keys.S) _isDownS = isDown;
            if (e.KeyCode == System.Windows.Forms.Keys.D) _isDownD = isDown;

            // Déterminer la "zone"
            int nbKeyPressed = (_isDownW ? 1 : 0) + (_isDownA ? 1 : 0) + (_isDownS ? 1 : 0) + (_isDownD ? 1 : 0);
            if (nbKeyPressed <= 0 || nbKeyPressed > 2) _edgeZoneKeyboard = EdgeZone.NONE;
            else if (_isDownA && _isDownW) _edgeZoneKeyboard = EdgeZone.LT;
            else if (_isDownA && _isDownS) _edgeZoneKeyboard = EdgeZone.LB;
            else if (_isDownD && _isDownW) _edgeZoneKeyboard = EdgeZone.RT;
            else if (_isDownD && _isDownS) _edgeZoneKeyboard = EdgeZone.RB;
            else if (_isDownA) _edgeZoneKeyboard = EdgeZone.L;
            else if (_isDownD) _edgeZoneKeyboard = EdgeZone.R;
            else if (_isDownW) _edgeZoneKeyboard = EdgeZone.T;
            else if (_isDownS) _edgeZoneKeyboard = EdgeZone.B;

            // Annuler l'événement original
            e.Handled = true;
        }

        private void SendKey(KEYS_DX key, bool isDown)
        {
            switch (key)
            {
                case KEYS_DX.ALT: _isDownAlt = isDown; break;
                case KEYS_DX.LEFT: _isDownLeft = isDown; break;
                case KEYS_DX.RIGHT: _isDownRight = isDown; break;
                case KEYS_DX.UP: _isDownTop = isDown; break;
                case KEYS_DX.BOTTOM: _isDownBottom = isDown; break;
            }

            short Keycode = (short)key;
            int KeyUporDown;
            if (isDown) KeyUporDown = 0x100;
            else KeyUporDown = 2;

            INPUT[] InputData = new INPUT[1];

            InputData[0].type = 1;
            InputData[0].ki.wScan = Keycode;
            InputData[0].ki.dwFlags = KeyUporDown;
            InputData[0].ki.time = 0;
            InputData[0].ki.dwExtraInfo = IntPtr.Zero;

            SendInput(1, InputData, Marshal.SizeOf(typeof(INPUT)));
        }
        #endregion
    }
}