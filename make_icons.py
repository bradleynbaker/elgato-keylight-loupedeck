"""
Generates ElgatoKeyLight plugin icons.
Design: dark bg, warm amber circle (the light), white 'K' in center.
"""
from PIL import Image, ImageDraw, ImageFont
import os

OUT = os.path.join(os.path.dirname(__file__), "images")
os.makedirs(OUT, exist_ok=True)

BG      = (18, 18, 18)
AMBER   = (255, 190, 60)
WHITE   = (255, 255, 255)
RING    = (255, 220, 120)

def make_icon(size: int, path: str):
    img  = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Dark rounded square background
    r = size // 5
    draw.rounded_rectangle([0, 0, size - 1, size - 1], radius=r, fill=BG)

    # Outer glow ring
    pad   = size * 0.12
    glow  = size * 0.06
    draw.ellipse([pad - glow, pad - glow, size - pad + glow, size - pad + glow],
                 fill=(*RING, 60))

    # Amber circle (the light)
    draw.ellipse([pad, pad, size - pad, size - pad], fill=AMBER)

    # White 'K' label — scale font to icon size
    font_size = max(6, int(size * 0.38))
    try:
        font = ImageFont.truetype("arial.ttf", font_size)
    except OSError:
        font = ImageFont.load_default(size=font_size)

    bbox = draw.textbbox((0, 0), "K", font=font)
    tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
    x = (size - tw) // 2 - bbox[0]
    y = (size - th) // 2 - bbox[1]
    draw.text((x, y), "K", fill=WHITE, font=font)

    img.save(path)
    print(f"  {os.path.basename(path)} ({size}x{size})")

sizes = [16, 32, 48, 256]
for s in sizes:
    make_icon(s, os.path.join(OUT, f"PluginIcon{s}x{s}.png"))

print("Done.")
