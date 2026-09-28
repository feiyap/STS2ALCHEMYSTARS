# 将空裔多人手图从横向（指左）重排为原版竖向（指上）422x1200，并对齐指向锚点 (163, 10)。
from __future__ import annotations

import shutil
from pathlib import Path

from PIL import Image

SRC_DIR = Path(__file__).resolve().parents[1] / "AlchemyStars" / "images" / "characters"
BACKUP_DIR = SRC_DIR / "_hand_backup_horizontal"

NAMES = [
    "AlchemyStars_hand_point.png",
    "AlchemyStars_hand_rock.png",
    "AlchemyStars_hand_paper.png",
    "AlchemyStars_hand_scissors.png",
]

CANVAS = (422, 1200)
PIVOT_X = 163
ALPHA_CUTOFF = 16
# 宽度必须留出把指尖放到 x=163 的空间（原版内容宽约 280–330）。
TARGET_CONTENT_WIDTH = 300
TARGET_CONTENT_HEIGHT = 1100


def opaque_bbox(im: Image.Image, cutoff: int = ALPHA_CUTOFF) -> tuple[int, int, int, int] | None:
    pixels = im.load()
    width, height = im.size
    left, top, right, bottom = width, height, -1, -1
    for y in range(height):
        for x in range(width):
            if pixels[x, y][3] > cutoff:
                if x < left:
                    left = x
                if y < top:
                    top = y
                if x > right:
                    right = x
                if y > bottom:
                    bottom = y
    if right < 0:
        return None
    return left, top, right + 1, bottom + 1


def top_mid_x(im: Image.Image, cutoff: int = ALPHA_CUTOFF) -> tuple[int, float]:
    pixels = im.load()
    for y in range(im.height):
        xs = [x for x in range(im.width) if pixels[x, y][3] > cutoff]
        if xs:
            return y, sum(xs) / len(xs)
    return 0, im.width / 2


def main() -> None:
    BACKUP_DIR.mkdir(exist_ok=True)

    for name in NAMES:
        src = SRC_DIR / name
        bak = BACKUP_DIR / name
        if not bak.exists():
            shutil.copy2(src, bak)

        im = Image.open(bak).convert("RGBA")
        # 原图手指朝左 → 顺时针 90° 后手指朝上。
        im = im.rotate(-90, expand=True)

        bbox = opaque_bbox(im)
        if bbox is None:
            raise RuntimeError(f"empty alpha: {name}")
        im = im.crop(bbox)

        scale = min(TARGET_CONTENT_WIDTH / im.width, TARGET_CONTENT_HEIGHT / im.height)
        new_w = max(1, int(round(im.width * scale)))
        new_h = max(1, int(round(im.height * scale)))
        im = im.resize((new_w, new_h), Image.Resampling.LANCZOS)

        # 缩放后可能重新出现半透明边缘，再裁一次。
        bbox = opaque_bbox(im)
        if bbox is None:
            raise RuntimeError(f"empty after resize: {name}")
        im = im.crop(bbox)

        top_y, mid_x = top_mid_x(im)
        target_top_y = 12 if "point" in name else 140
        paste_x = int(round(PIVOT_X - mid_x))
        paste_y = int(round(target_top_y - top_y))
        paste_x = max(0, min(paste_x, CANVAS[0] - im.width))
        paste_y = max(0, min(paste_y, CANVAS[1] - im.height))

        canvas = Image.new("RGBA", CANVAS, (0, 0, 0, 0))
        canvas.alpha_composite(im, (paste_x, paste_y))
        canvas.save(src)

        verify_y, verify_x = top_mid_x(canvas)
        print(
            f"{name}: content={im.size} paste=({paste_x},{paste_y}) "
            f"top=({verify_x:.1f},{verify_y})"
        )

    print("done. backups:", BACKUP_DIR)


if __name__ == "__main__":
    main()
