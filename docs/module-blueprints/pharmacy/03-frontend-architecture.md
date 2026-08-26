# Farmasi — Arsitektur Frontend Routing Depo

Status: `approved` oleh product/domain owner pada 21 Agustus 2026.

Tidak ada layar, route, menu, atau Redux state baru untuk routing. Pemilihan Depo berjalan di backend. UI workflow Farmasi hanya perlu menampilkan pesan backend ketika konfigurasi tidak ditemukan atau ambigu, mempertahankan tombol coba lagi, dan tidak menawarkan pemilihan Depo manual sebagai fallback.

| Keadaan | Perilaku UI | Wewenang |
| --- | --- | --- |
| Routing berhasil | Lanjutkan workflow tanpa dialog pemilihan | Backend authoritative |
| Tidak ada kandidat | Tampilkan pesan konfigurasi dan hentikan proses | Backend authoritative |
| Kandidat ganda | Tampilkan pesan konfigurasi dan hentikan proses | Backend authoritative |
| Gangguan jaringan | Tampilkan retry; jangan menganggap routing berhasil | Pola project existing |

Teks visual, posisi alert, dan komponen pesan adalah `DEV_DISCRETION` selama memakai design system existing. Menampilkan seluruh kandidat lokasi kepada pengguna tidak diizinkan karena dapat membocorkan konfigurasi dan mendorong fallback manual.
