# Permintaan ke Pemilik CRM-Membership dan Billing — Member Aktif untuk Demo Pasien Prioritas

| Field | Value |
|---|---|
| `request_id` | `IGD-REQ-005` |
| `tanggal` | 2026-10-18 |
| `pengaju` | Rizki Gunawan — Product/Domain Owner IGD (`IGD-DEC-089`) |
| `penerima` | **Yoga Aji Pratama** — Product/Domain Owner CRM-Membership; **owner Billing/Kasir** |
| `rujukan` | `IGD-FACT-150`, `IGD-OQ-137`; CRM `CRM-DEC-004`, `CRM-DEC-019`, `CRM-DEC-020`; [penilaian Putaran P1](../testing/2026-10-18-laporan-uji-putaran-p1.md) |
| `status` | **`menunggu jawaban`** |
| `sifat` | Operasional, lintas modul. **Bukan** artefak desain IGD |

---

## 1. Satu paragraf untuk yang tidak punya waktu

Demo Selasa 20 dan Rabu 21 Oktober 2026 memperagakan pasien member (VIP/Platinum) di IGD: reservasi pra-kedatangan
mengenali tier pasien, dan triage tetap mengikuti kegawatan. Untuk itu dibutuhkan **pasien dengan keanggotaan `Active`**.
Di dev hari ini keanggotaan baru selalu lahir _Menunggu Pembayaran_ (`CRM-DEC-004`), perubahan status manual dijawab
`410 Gone`, dan jalur kasir yang mencatat iuran lalu mengaktifkan keanggotaan (`CRM-DEC-019`) belum tersambung. Akibatnya
**member aktif tidak dapat disiapkan lewat aplikasi**. Pada uji 18 Oktober, penguji mengaktifkan satu member lewat SQL —
cara yang tidak sah dan tidak kami pakai untuk demo.

_Contoh._ Petugas mendaftarkan Tn. Hadi sebagai _Platinum_ pukul 09.00; statusnya _Menunggu Pembayaran_. Karena kasir belum
dapat mencatat iurannya, reservasi IGD Tn. Hadi tidak menampilkan lencana Platinum, dan setelah 7 hari keanggotaannya batal
otomatis (`CRM-DEC-014`).

---

## 2. Yang TIDAK diminta

| Hal | Sikap |
|---|---|
| Mengubah aturan `CRM-DEC-004` (aktif sesudah lunas) | Tidak — aturan itu tetap |
| Diskon membership di billing IGD (`IGD-DEC-317`) | Tidak — permintaan terpisah, menunggu aturan Billing dan CRM |
| SQL langsung oleh tim IGD atau penguji | Tidak — `IGD-FACT-148` |

---

## 3. Permintaan

| No | Permintaan | Untuk | Dasar |
|---|---|---|---|
| 1 | **Kapan jalur iuran → aktif tersedia** di dev: kasir mencatat iuran membership lunas lewat transaksi non-kunjungan, lalu keanggotaan menjadi `Active` | Owner Billing, Yoga Aji Pratama | `CRM-DEC-019`, `CRM-DEC-020` |
| 2 | **Cara resmi menyiapkan member aktif untuk demo** 20–21 Oktober bila jalur nomor 1 belum siap — misalnya data demo yang disiapkan pemilik data dan dicatat sebagai persiapan demo, bukan bukti uji | Yoga Aji Pratama (pemilik data membership) | `IGD-OQ-137` |
| 3 | **Sebutkan pasien member aktif yang boleh dipakai demo**, bila sudah ada di dev | Yoga Aji Pratama | — |

---

## 4. Untuk agent Claude di sisi CRM atau Billing

Berkas ini adalah titik sambung antara blueprint IGD dan blueprint **`crm-membership`** (pemilik Yoga Aji Pratama) serta
**`billing-kasir`**. Bila owner meminta agent mengerjakan permintaan ini, agent tersebut:

1. Membaca berkas ini, `CRM-DEC-004`, `CRM-DEC-014`, `CRM-DEC-019`, `CRM-DEC-020` di `crm-membership/00-interview-decisions.md`,
   lalu bukti IGD: [penilaian P1](../testing/2026-10-18-laporan-uji-putaran-p1.md) dan
   [catatan bukti uji](../evidence/2026-10-18-catatan-bukti-uji-p1-k1-dan-serah-terima.md). Bukti mentahnya ada di laptop
   pemilik IGD dan **sudah dinilai**; fakta `IGD-FACT-150` tidak perlu diuji ulang.
2. Mencatat sikap CRM atau Billing di blueprint **modul masing-masing** — decision log dan, bila perlu, `approval-requests/` —
   memakai penomoran sendiri (`CRM-…`, `BKC-…`) dengan rujukan silang `IGD-REQ-005`, `IGD-OQ-137`, `IGD-FACT-150`. Bila
   jalur iuran → aktif dijadwalkan, sebutkan task ID-nya di bagian 5.
3. **Tidak** menulis berkas lain di `docs/module-blueprints/igd/**` selain tabel jawaban di bagian 5, tidak menulis tabel
   `Emg*`, tidak mengubah alur IGD, dan **tidak** menyiapkan member aktif lewat SQL.
4. Mencatat jawaban di bagian 5 berkas ini, atau mengirimkannya ke Rizki untuk dicatat.

Bagian Billing yang tidak menyangkut membership ada di [`IGD-REQ-006`](2026-10-18-catatan-pemilik-billing-hasil-uji-tagihan-igd.md).

---

## 5. Jawaban pemilik

| Butir | Jawaban | Siapa / tanggal |
|---|---|---|
| 1 — jalur iuran → aktif | | |
| 2 — cara resmi untuk demo | | |
| 3 — pasien member aktif yang boleh dipakai | | |

Kolom tambahan untuk agent CRM/Billing: tuliskan rujukan keputusan di blueprint modul Anda (mis. `CRM-DEC-…`, `BKC-DEC-…`,
atau task ID) pada kolom _Jawaban_, supaya agent IGD dapat menelusurinya.
