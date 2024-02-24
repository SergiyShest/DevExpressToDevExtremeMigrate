<template >
  <!--  -->
  <div style="width: 100%;" >
    <v-progress-linear
      v-if="item"
      :value="item.PercentFilling"
      :color="ColorForPercent(item)"
      height="100%"
      ><div
        style="
          display: flex;
          flex-direction: row;
          justify-content: flex-start;
          align-items: flex-start;
          width: 100%;
        "
      >
        <span v-if="showPath">{{ item.Path }}</span>

        <span style="margin-left: 20px"> all:{{ item.AllContentCount }}</span>
        <v-spacer />
        <strong >{{ formatNumber(item.PercentFilling) }}%</strong
        >
      </div>
    </v-progress-linear>
  </div>
</template>
    <script>
export default {
  name: "RNode",
  props: ["item","showPath"],
  data: () => ({}),

  methods: {
    formatNumber(percent) {
      var num = new Intl.NumberFormat("en", {
        maximumFractionDigits: 2,
      }).format(percent);

      return num;
    },
    // path(item) {
    //   if (!item) return;
    //   let path = item.Path.replace(/\/\w+$/, "");
    //   return path;
    // },
    itemPercent(item) {
      if (!item) return 0;
      if (!item.PercentFilling) return 0;
      var num = parseInt(item.PercentFilling, 10);
      return num;
    },
    ColorForPercent(item) {
      if (!item) return;
      const percent = item.PercentFilling;
      var color;
      if (percent <= 80) {
        color = "green";
      } else if (percent <= 100) {
        color = "yellow";
      } else {
        color = "red";
      }

      return color;
    },
  },
  mounted: function () {
    console.log(this.item.Name)
  },
};
</script>      